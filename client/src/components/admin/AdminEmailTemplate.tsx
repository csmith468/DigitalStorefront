import { useState } from 'react';

  export function AdminEmailTemplate() {
    const [subject, setSubject] = useState('Thanks for Testing Digital Storefront!');
    const [orderDetails, setOrderDetails] = useState('Order #1234 ($9.99) - Sample Product');

    const emailHtml = `
      <div style="font-family: 'Quicksand', Arial, sans-serif; max-width: 600px; margin: 0 auto; background: #FAF5FF;">
        <div style="background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); color: white; padding: 30px; text-align: center; border-radius: 12px 12px 0 0;">
          <h1 style="margin: 0; font-size: 28px; font-weight: 600;">Digital Storefront</h1>
        </div>
        <div style="padding: 30px; background: #FFFFFF; border: 1px solid #E9D5FF; border-top: none;">
          <h2 style="color: #4C1D95; margin-top: 0;">${subject}</h2>
          <p style="color: #4C1D95; line-height: 1.6; background: #F3E8FF; padding: 15px; border-radius: 8px; border: 1px solid #E9D5FF;">${orderDetails}</p>
          <p style="color: #6B7280; line-height: 1.6; margin-top: 20px;">
            Thanks for testing out my demo e-commerce platform! This project showcases
            full-stack development with .NET 8, React, SQL Server, and Azure.
          </p>
          <p style="color: #6B7280; line-height: 1.6;">
            Feel free to explore more features or check out my other work below.
          </p>
          <div style="margin-top: 30px; text-align: center;">
            <a href="https://digitalstorefront.dev"
              style="display: inline-block; background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); color: white;
                padding: 12px 24px; text-decoration: none; border-radius: 8px;
                margin-right: 10px; font-weight: 600;">
              Back to Site
            </a>
            <a href="https://github.com/csmith468/DigitalStorefront"
              style="display: inline-block; background: #4C1D95; color: white;
                padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: 600;">
              View GitHub
            </a>
          </div>
        </div>
        <div style="padding: 20px; text-align: center; color: #6B7280; font-size: 12px; background: #FAF5FF; border-radius: 0 0 12px 12px;">
          <p style="margin: 0 0 5px 0;">Digital Storefront - Full-Stack Portfolio Project</p>
          <p style="margin: 0;">Built with .NET 8, React, SQL Server, and Azure</p>
        </div>
      </div>
    `;

    return (
      <div className="space-y-6">
        <div className="bg-white rounded-lg border border-[var(--color-border)] p-6">
          <h3 className="text-lg font-semibold text-[var(--color-text-primary)] mb-4">
            Email Template Editor
          </h3>
          <p className="text-sm text-gray-500 mb-4">
            Customize the order confirmation email sent to customers.
          </p>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Subject</label>
              <input
                type="text"
                value={subject}
                onChange={(e) => setSubject(e.target.value)}
                className="w-full border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-purple-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Order Details (dynamic)</label>
              <input
                type="text"
                value={orderDetails}
                onChange={(e) => setOrderDetails(e.target.value)}
                className="w-full border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-purple-500"
              />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg border border-[var(--color-border)] p-6">
          <h3 className="text-lg font-semibold text-[var(--color-text-primary)] mb-4">
            Live Preview
          </h3>
          <div 
            className="border border-gray-200 rounded-lg overflow-hidden"
            dangerouslySetInnerHTML={{ __html: emailHtml }}
          />
        </div>
      </div>
    );
  }