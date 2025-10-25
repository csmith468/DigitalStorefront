import { ArrowLeftIcon } from "@heroicons/react/24/outline";
import { useNavigate } from "react-router-dom";

interface PageHeaderProps {
  title: string;
  subtitle?: string
  returnLink?: string;
  returnText?: string;
}
 
export const PageHeader: React.FC<PageHeaderProps> = ({
  title, returnLink, returnText, subtitle
}) => {
  const navigate = useNavigate();

  return (
    <div className="mb-8">
      {returnLink && (
        <button
          onClick={() => navigate(`${returnLink}`)}
          className="flex items-center gap-2 text-gray-600 hover:text-[var(--color-primary)] mb-4 transition-colors group"
        >
          <ArrowLeftIcon className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
          <span className="font-medium">{returnText || 'Go Back'}</span>
        </button>
      )}

      <h1 className="text-4xl font-bold text-text-primary">{title}</h1>
      <div className="w-24 h-1 rounded-full mt-4" 
        style={{ background: `linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%)` }}
      ></div>
      
      {subtitle && (
        <p className="text-lg text-text-secondary mt-4">{subtitle}</p>
      )}
    </div>
  );
}