import * as signalR from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";
import { logger } from "../../utils/logger";

const SIGNALR_URL = import.meta.env.VITE_API_URL || "http://localhost:5000";

interface UseProductViewersResult {
  viewerCount: number | null;
  isConnected: boolean;
}

export function useProductViewers(productSlug: string | undefined): UseProductViewersResult {
  const [viewerCount, setViewerCount] = useState<number | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!productSlug) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${SIGNALR_URL}/hubs/product-viewers`)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on('ViewerCountUpdated', (count: number) => {
      setViewerCount(count);
    });

    connection.onreconnecting(() => {
      setIsConnected(false);
      logger.info('SignalR reconnecting...');
    });

    connection.onreconnected(() => {
      setIsConnected(true);
      logger.info('SignalR reconnected');
      connection.invoke('JoinProductAsync', productSlug).catch(err => {
        logger.error('Failed to rejoin product group:', err);
      });
    });

    connection.onclose(() => { 
      setIsConnected(false); 
    });

    connection.start()
      .then(() => {
        setIsConnected(true);
        return connection.invoke('JoinProductAsync', productSlug);
      }).catch(err => { 
        logger.error('SignalR connection failed:', err);
      });

    return () => { connection.stop(); };
  }, [productSlug]);

  return { viewerCount, isConnected };
}