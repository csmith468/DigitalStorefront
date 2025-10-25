interface LoadingScreenProps {
  message?: string;
}

export function LoadingScreen ({
  message = "Loading...",
}: LoadingScreenProps) {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-center items-center min-h-[400px]">
        <div className="text-lg text-text-secondary">{message}</div>
      </div>
    </div>
  );
};
