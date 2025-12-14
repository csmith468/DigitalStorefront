using API.Configuration;
using API.Database;
using API.Models;
using API.Models.DsfTables;
using API.Models.Dtos;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace API.Services.Notifications;

public interface IEmailDeliveryService
{
    Task<Result<int>> CreateAndSendAsync(CreateNotificationRequest request, CancellationToken ct = default);
}

public class EmailDeliveryService : IEmailDeliveryService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISendGridClient _sendGridClient;
    private readonly SendGridOptions _sendGridOptions;
    private readonly ILogger<EmailDeliveryService> _logger;

    public EmailDeliveryService(IQueryExecutor queryExecutor, ICommandExecutor commandExecutor, ISendGridClient sendGridClient,
        IOptions<SendGridOptions> sendGridOptions, ILogger<EmailDeliveryService> logger)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _sendGridClient = sendGridClient;
        _sendGridOptions = sendGridOptions.Value;
        _logger = logger;
    }

    public async Task<Result<int>> CreateAndSendAsync(CreateNotificationRequest request, CancellationToken ct = default)
    {
        if (request.Order.Email == null)
            return Result<int>.Failure(ErrorMessages.Order.NoEmail);

        var deliveryId = await _commandExecutor.InsertAsync(new EmailDelivery
        {
            OrderId = request.Order.OrderId,
            Email = request.Order.Email,
            Subject = request.Subject,
            Body = request.Body
        }, ct);

        return await SendAsync(deliveryId, ct);
    }

    private async Task<Result<int>> SendAsync(int emailDeliveryId, CancellationToken ct = default)
    {
        var delivery = await _queryExecutor.GetByIdAsync<EmailDelivery>(emailDeliveryId, ct);
        if (delivery!.Status == "Sent")
            return Result<int>.Success(emailDeliveryId); // already sent - idempotent

        delivery.Status = "Sending";
        delivery.AttemptCount++;
        delivery.LastAttemptAt = DateTime.UtcNow;
        await _commandExecutor.UpdateAsync(delivery, delivery.UpdatedAt, ct);

        try
        {
            var message = new SendGridMessage
            {
                From = new EmailAddress(_sendGridOptions.FromEmail, _sendGridOptions.FromName),
                Subject = delivery.Subject,
                PlainTextContent = delivery.Body,
                HtmlContent = BuildHtmlEmail(delivery)
            };
            message.AddTo(delivery.Email);

            var response = await _sendGridClient.SendEmailAsync(message, ct);

            if (response.IsSuccessStatusCode)
            {
                delivery.Status = "Sent";
                delivery.SentAt = DateTime.UtcNow;
                _logger.LogInformation("Email sent to {Email} for notification {NotificationId}", delivery.Email,
                    delivery.EmailDeliveryId);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(ct);
                throw new Exception($"SendGrid returned {response.StatusCode}: {body}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", delivery.Email);
            delivery.Status = "Failed";
            delivery.FailedReason = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
        }

        var latestDelivery = await _queryExecutor.GetByIdAsync<EmailDelivery>(emailDeliveryId, ct);
        await _commandExecutor.UpdateAsync(delivery, latestDelivery!.UpdatedAt, ct);

        return delivery.FailedReason != null
            ? Result<int>.Failure(ErrorMessages.Email.SendFailed)
            : Result<int>.Success(emailDeliveryId);
    }

    private string BuildHtmlEmail(EmailDelivery delivery)
    {
        return $@"
            <html>
                <body style=""font-family: 'Quicksand', Arial, sans-serif; max-width: 600px; margin: 0 auto; background: #FAF5FF;"">
                    <div style=""background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); color: white; padding: 30px; text-align: center; border-radius: 12px 12px 0 0;"">
                        <h1 style=""margin: 0; font-size: 28px; font-weight: 600;"">Digital Storefront</h1>
                    </div>
                    <div style=""padding: 30px; background: #FFFFFF; border: 1px solid #E9D5FF; border-top: none;"">
                        <h2 style=""color: #4C1D95; margin-top: 0;"">{delivery.Subject}</h2>
                        <p style=""color: #4C1D95; line-height: 1.6; background: #F3E8FF; padding: 15px; border-radius: 8px; border: 1px solid #E9D5FF;"">{delivery.Body}</p>
                        <p style=""color: #6B7280; line-height: 1.6; margin-top: 20px;"">
                            Thanks for testing out my demo e-commerce platform! This project showcases
                            full-stack development with .NET 8, React, SQL Server, and Azure.
                        </p>
                        <p style=""color: #6B7280; line-height: 1.6;"">
                            Feel free to explore more features or check out my other work below.
                        </p>
                        <div style=""margin-top: 30px; text-align: center;"">
                            <a href=""https://digitalstorefront.dev""
                                style=""display: inline-block; background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); color: white;
                                    padding: 12px 24px; text-decoration: none; border-radius: 8px;
                                    margin-right: 10px; font-weight: 600;"">
                                Back to Site
                            </a>
                            <a href=""https://github.com/csmith468/DigitalStorefront""
                                style=""display: inline-block; background: #4C1D95; color: white;
                                    padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: 600;"">
                                View GitHub
                            </a>
                        </div>
                    </div>
                    <div style=""padding: 20px; text-align: center; color: #6B7280; font-size: 12px; background: #FAF5FF; border-radius: 0 0 12px 12px;"">
                        <p style=""margin: 0 0 5px 0;"">Digital Storefront - Full-Stack Portfolio Project</p>
                        <p style=""margin: 0;"">Built with .NET 8, React, SQL Server, and Azure</p>
                    </div>
                </body>
            </html>";
    }
}