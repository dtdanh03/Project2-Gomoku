package com.example.demo_wifip2p;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.net.NetworkInfo;
import android.net.wifi.p2p.WifiP2pManager;
import android.net.wifi.p2p.WifiP2pManager.ActionListener;
import android.net.wifi.p2p.WifiP2pManager.Channel;
import android.net.wifi.p2p.WifiP2pManager.ConnectionInfoListener;
import android.net.wifi.p2p.WifiP2pManager.PeerListListener;
import android.widget.Toast;

public class WifiP2PBroardcast extends BroadcastReceiver{

	WifiP2pManager mManager;
	Channel mChannel;
	MainActivity mActitity;
	
	public WifiP2PBroardcast(WifiP2pManager manager, Channel channel, MainActivity activity){
		mManager = manager;
		mChannel = channel;
		mActitity = activity;
	}
	
	@Override
	public void onReceive(Context context, Intent intent) {
		// TODO Auto-generated method stub
		String action = intent.getAction();

        if (WifiP2pManager.WIFI_P2P_STATE_CHANGED_ACTION.equals(action)) {
            // Check to see if Wi-Fi is enabled and notify appropriate activity
        	
        	int state = intent.getIntExtra(WifiP2pManager.EXTRA_WIFI_STATE, -1);
        	if (state == WifiP2pManager.WIFI_P2P_STATE_ENABLED){
        		if (mActitity.firstDiscover == true){
        			//((WifiP2PBroadcastListener)mActitity).changeState();
        		}
        		//Toast.makeText(mActitity.getApplicationContext(), "Wifi direct enable", Toast.LENGTH_SHORT).show();
        	}else{
        		//mActitity.firstDiscover = true;
        		//Toast.makeText(mActitity.getApplicationContext(), "Wifi direct un-enable", Toast.LENGTH_SHORT).show();
        	}
        } else if (WifiP2pManager.WIFI_P2P_PEERS_CHANGED_ACTION.equals(action)) {
            // Call WifiP2pManager.requestPeers() to get a list of current peers
        	//Toast.makeText(mActitity.getApplicationContext(), "Peer change", Toast.LENGTH_SHORT).show();
        	
        	if (mManager != null){
        		mManager.requestPeers(mChannel, (PeerListListener)mActitity);
        	}
        } else if (WifiP2pManager.WIFI_P2P_CONNECTION_CHANGED_ACTION.equals(action)) {
            // Respond to new connection or disconnections
        	Toast.makeText(mActitity.getApplicationContext(), "Connection change", Toast.LENGTH_SHORT).show();
        	
        	NetworkInfo networkInfo = (NetworkInfo)intent.getParcelableExtra(WifiP2pManager.EXTRA_NETWORK_INFO);
        	
        	if (networkInfo.isConnected())
        		mManager.requestConnectionInfo(mChannel, (ConnectionInfoListener)mActitity);
        } else if (WifiP2pManager.WIFI_P2P_THIS_DEVICE_CHANGED_ACTION.equals(action)) {
            // Respond to this device's wifi state changing
        } else if (WifiP2pManager.WIFI_P2P_DISCOVERY_CHANGED_ACTION.equals(action)){
        	//Toast.makeText(mActitity, "Discovery start", Toast.LENGTH_SHORT).show();
        	
        	int discoverState = intent.getIntExtra(WifiP2pManager.EXTRA_DISCOVERY_STATE, -1);
        	
        	if (discoverState == WifiP2pManager.WIFI_P2P_DISCOVERY_STARTED){
        		Toast.makeText(mActitity, "Discovery start", Toast.LENGTH_SHORT).show();
        	}else if (discoverState == WifiP2pManager.WIFI_P2P_DISCOVERY_STOPPED){		
        		Toast.makeText(mActitity, "Discovery stop", Toast.LENGTH_SHORT).show();
        	}
        }


	}
	
	public interface WifiP2PBroadcastListener{
		void changeState();
	}
}


