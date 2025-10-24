import React from "react";

interface LoadingScreenProps {
  message?: string;
}

export const LoadingScreen: React.FC<LoadingScreenProps> = ({
  message = "Loading...",
}) => {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-center items-center min-h-[400px]">
        <div className="text-lg text-text-secondary">{message}</div>
      </div>
    </div>
  );
};
