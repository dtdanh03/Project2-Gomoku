package com.example.demo_wifip2p;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.example.demo_wifip2p.ReceiveSocketAsync.SocketReceiverDataListener;
import com.example.demo_wifip2p.ServerSendSocket_Thread.ServerSocketListener;
import com.example.demo_wifip2p.WifiP2PBroardcast.WifiP2PBroadcastListener;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.net.wifi.p2p.WifiP2pConfig;
import android.net.wifi.p2p.WifiP2pDevice;
import android.net.wifi.p2p.WifiP2pDeviceList;
import android.net.wifi.p2p.WifiP2pInfo;
import android.net.wifi.p2p.WifiP2pManager;
import android.net.wifi.p2p.WifiP2pManager.ActionListener;
import android.net.wifi.p2p.WifiP2pManager.Channel;
import android.net.wifi.p2p.WifiP2pManager.ConnectionInfoListener;
import android.net.wifi.p2p.WifiP2pManager.DnsSdServiceResponseListener;
import android.net.wifi.p2p.WifiP2pManager.DnsSdTxtRecordListener;
import android.net.wifi.p2p.WifiP2pManager.PeerListListener;
import android.net.wifi.p2p.nsd.WifiP2pDnsSdServiceInfo;
import android.net.wifi.p2p.nsd.WifiP2pDnsSdServiceRequest;
import android.os.Bundle;
import android.os.Handler;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.View.OnClickListener;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.ToggleButton;

public class MainActivity extends Activity implements PeerListListener, ConnectionInfoListener, 
													  OnClickListener, WifiP2PBroadcastListener, 
													  SocketReceiverDataListener, ServerSocketListener{

	private static final int SOCKET_TIMEOUT = 5000;
	
	WifiP2pManager mManager;
	Channel mChannel;
	WifiP2PBroardcast mBroadcast;
	IntentFilter filter = new IntentFilter();
	
	Button btnSearch;
	Button btnConnect;
	Button btnSendImage;

	ListView lvDevice;
	DeviceListAdapter deviceListAdapter;
	
	TextView tvConnectInfo;
	
	ImageView imvReceiveImage;
	
	ToggleButton tgbtnAvailable;
	
	List<WifiP2pDevice> mPeerList = new ArrayList<WifiP2pDevice>();
	
	public ProgressDialog mProgess;
	
	WifiP2pDnsSdServiceInfo serviceInfo;
	
	String DETECT = "Demo_WifiP2P";
	String STATEDETECT = "OFF";
	
	ServerSendSocket_Thread serverSendThread = null;
	ServerReceiveSocket_Thread serverReceiveThread = null;
	
	Socket mSendSocket = null;
	Socket mReceiveSocket = null;
	
	public boolean firstDiscover = true;
	
	static final int PORT = 9000;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		btnSearch = (Button)findViewById(R.id.btnSearch);
		btnConnect = (Button)findViewById(R.id.btnConnect);
		btnSendImage = (Button)findViewById(R.id.btnSendImage);
		
		lvDevice = (ListView)findViewById(R.id.lvDevice);
		deviceListAdapter = new DeviceListAdapter(this, R.layout.listview_item, mPeerList);
		lvDevice.setAdapter(deviceListAdapter);
		
		tvConnectInfo = (TextView)findViewById(R.id.tvConnectInfo);
		
		imvReceiveImage = (ImageView)findViewById(R.id.imvReceiveFile);
		
		tgbtnAvailable = (ToggleButton)findViewById(R.id.tgbtnAvailable);
		
		tgbtnAvailable.setChecked(false);
		
		tgbtnAvailable.setOnCheckedChangeListener(new OnCheckedChangeListener() {
			
			@Override
			public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
				// TODO Auto-generated method stub
				if (tgbtnAvailable.isChecked()){
					STATEDETECT = "ON";
				}else{
					STATEDETECT = "OFF";
				}
				
				if (serviceInfo != null)
					mManager.removeLocalService(mChannel, serviceInfo, new ActionListener() {
					
						@Override
						public void onSuccess() {
							// TODO Auto-generated method stub
							
						}
						
						@Override
						public void onFailure(int reason) {
							// TODO Auto-generated method stub
							
						}
					});
				
				startRegistration();
			}
		});
		
		mProgess = new ProgressDialog(this){
			@Override
			public void onBackPressed() {
				if (mProgess.isShowing()){
					mProgess.dismiss();
				}
			};
		};
		
		btnSearch.setOnClickListener(this);
		btnConnect.setOnClickListener(this);
		btnSendImage.setOnClickListener(this);
	
		setManager();
	}
	
	private void setManager(){			
		mManager = (WifiP2pManager) getSystemService(WIFI_P2P_SERVICE);
		
		mChannel = mManager.initialize(this, getMainLooper(), null);
		
		mBroadcast = new WifiP2PBroardcast(mManager, mChannel, this);
		
		filter.addAction(WifiP2pManager.WIFI_P2P_STATE_CHANGED_ACTION);
		filter.addAction(WifiP2pManager.WIFI_P2P_PEERS_CHANGED_ACTION);
		filter.addAction(WifiP2pManager.WIFI_P2P_CONNECTION_CHANGED_ACTION);
		filter.addAction(WifiP2pManager.WIFI_P2P_THIS_DEVICE_CHANGED_ACTION);
		filter.addAction(WifiP2pManager.WIFI_P2P_DISCOVERY_CHANGED_ACTION);
		
		startRegistration();
	}
	
	private void startRegistration(){
		 //  Create a string map containing information about your service.
		discoverService();
        Map record = new HashMap();
        record.put("listenport", String.valueOf(9000));
        record.put("buddyname", "Mai Hung" + (int) (Math.random() * 1000));
        record.put("available", "visible");

        // Service information.  Pass it an instance name, service type
        // _protocol._transportlayer , and the map containing
        // information other devices will want once they connect to this one.
        serviceInfo = WifiP2pDnsSdServiceInfo.newInstance(DETECT, STATEDETECT, record);

        // Add the local service, sending the service info, network channel,
        // and listener that will be used to indicate success or failure of
        // the request.
        mManager.addLocalService(mChannel, serviceInfo, new ActionListener() {
            @Override
            public void onSuccess() {
                // Command successful! Code isn't necessarily needed here,
                // Unless you want to update the UI or add logging statements.
            }

            @Override
            public void onFailure(int arg0) {
                // Command failed.  Check for P2P_UNSUPPORTED, ERROR, or BUSY
            }
        });

	}
	
	final HashMap<String, String> buddies = new HashMap<String, String>();
	
	int i = 0;

	private void discoverService(){
		
		DnsSdTxtRecordListener txtListener = new DnsSdTxtRecordListener() {
	        @Override
	        /* Callback includes:
	         * fullDomain: full domain name: e.g "printer._ipp._tcp.local."
	         * record: TXT record dta as a map of key/value pairs.
	         * device: The device running the advertised service.
	         */

	        public void onDnsSdTxtRecordAvailable(
	                String fullDomain, Map record, WifiP2pDevice device) {
	            }
	        };
	        
	        DnsSdServiceResponseListener servListener = new DnsSdServiceResponseListener() {
	            @Override
	            public void onDnsSdServiceAvailable(String instanceName, String registrationType,
	                    WifiP2pDevice resourceType) {
	                            
	                    if (instanceName.equals(DETECT) && registrationType.equals("ON.local.")){
	                    	// Add to the custom adapter defined specifically for showing
		                    // wifi devices.
		                    	if (mProgess != null && mProgess.isShowing()){
		                			mProgess.dismiss();
		                			showPeer(resourceType);
		                    	}             
	                    }
	                    
	            }
	        };

	    mManager.setDnsSdResponseListeners(mChannel, servListener, txtListener);
		mManager.addServiceRequest(mChannel, WifiP2pDnsSdServiceRequest.newInstance(), new ActionListener() {
			
			@Override
			public void onSuccess() {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onFailure(int reason) {
				// TODO Auto-generated method stub
				
			}
		});
		
		mManager.discoverServices(mChannel, new ActionListener() {
			
			@Override
			public void onSuccess() {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onFailure(int reason) {
				// TODO Auto-generated method stub
				
			}
		});
	}
	
	@Override
	protected void onPause() {
		// TODO Auto-generated method stub
		super.onPause();
	}
	
	@Override
	protected void onResume() {
		// TODO Auto-generated method stub
		super.onResume();
		registerReceiver(mBroadcast, filter);
	}

	@Override
	public void onPeersAvailable(WifiP2pDeviceList peers) {
		// TODO Auto-generated method stub
	}
	
	@Override
	protected void onDestroy() {
		// TODO Auto-generated method stub
		super.onDestroy();
		mManager.removeLocalService(mChannel, serviceInfo, new ActionListener() {
			
			@Override
			public void onSuccess() {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onFailure(int reason) {
				// TODO Auto-generated method stub
				
			}
		});
		
		mManager.removeGroup(mChannel, new ActionListener() {
			
			@Override
			public void onSuccess() {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onFailure(int reason) {
				// TODO Auto-generated method stub
				
			}
		});
	}
	
	private void showPeer(WifiP2pDevice device){	
		if (firstDiscover == false){
			if (!mPeerList.contains(device)){
				mPeerList.add(device);
				deviceListAdapter.notifyDataSetChanged();
				btnConnect.setVisibility(View.VISIBLE);
			}
			
		}
//		mPeerList.clear();
//		mPeerList.addAll(peers.getDeviceList());
//		
//		if (mPeerList.size() > 0){
//			tvDeviceName.setText(mPeerList.get(0).deviceName);
//			btnConnect.setVisibility(View.VISIBLE);
//		}
	}
	
	private void sendImageInGalery(){
		Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
        intent.setType("image/*");
        startActivityForResult(intent, 1);
	}
	
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		// TODO Auto-generated method stub
		
		final Uri uri = data.getData();
	
		Thread send = new Thread(new Runnable() {

			@Override
			public void run() {

				ContentResolver cr = getContentResolver();
				InputStream is;
				OutputStream os;
				try {
					is = cr.openInputStream(uri);
					os = mSendSocket.getOutputStream();
					FileTransferService.copyFile(is, os);
				} catch (FileNotFoundException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}			
			}
		});
		send.start();
	}

	@Override
	public void onClick(View v) {
		// TODO Auto-generated method stub
		switch (v.getId()){
		case R.id.btnSearch:
			mManager.discoverPeers(mChannel, new ActionListener() {
				
				@Override
				public void onSuccess() {
					// TODO Auto-generated method stub
					
				}
				
				@Override
				public void onFailure(int reason) {
					// TODO Auto-generated method stub
					
				}
			});
			
			firstDiscover = false;
			
			mPeerList.clear();
			deviceListAdapter.notifyDataSetChanged();
			
			mProgess.setTitle("Discover Peer");
			mProgess.setMessage("Wait for discovery");
			mProgess.show();
			Handler handler = new Handler();
			handler.postDelayed(new Runnable() {
				
				@Override
				public void run() {
					// TODO Auto-generated method stub
					if (mProgess.isShowing()){
						mProgess.dismiss();
					}
				}
			}, 30000);			
			discoverService();
			break;
			
		case R.id.btnConnect:
			if (btnConnect.getText().equals("Connect")){
				WifiP2pDevice device = mPeerList.get(0);
				WifiP2pConfig config = new WifiP2pConfig();
				config.deviceAddress = device.deviceAddress;
				
				mManager.connect(mChannel, config, new ActionListener() {
					
					@Override
					public void onSuccess() {
						// TODO Auto-generated method stub
					}
					
					@Override
					public void onFailure(int reason) {
						// TODO Auto-generated method stub
						
					}
				});
			}else{
				btnConnect.setVisibility(View.GONE);
				btnSendImage.setVisibility(View.GONE);
				tvConnectInfo.setVisibility(View.GONE);
				
				mPeerList.clear();
				deviceListAdapter.notifyDataSetChanged();
				
				discoverService();
				
				mManager.removeGroup(mChannel, new ActionListener() {
					
					@Override
					public void onSuccess() {
						// TODO Auto-generated method stub
						btnConnect.setText("Connect");
						btnConnect.setVisibility(View.GONE);
						
						btnSendImage.setVisibility(View.GONE);
						
						tvConnectInfo.setVisibility(View.GONE);
						
					}
					
					@Override
					public void onFailure(int reason) {
						// TODO Auto-generated method stub
						
					}
				});	
			}
			break;
			
		case R.id.btnSendImage:
			sendImageInGalery();
		}
	}

	@Override
	public void onConnectionInfoAvailable(WifiP2pInfo info) {
		// TODO Auto-generated method stub
		
		btnConnect.setText("Disconnect");
		
		btnSendImage.setVisibility(View.VISIBLE);
		
		final String hostIP = info.groupOwnerAddress.getHostAddress();
		
		if (info.groupFormed){
			if (info.isGroupOwner){
				tvConnectInfo.setText("Group Owner: YES - IP: " + hostIP);
				try {
					if (serverSendThread == null && serverReceiveThread == null){
						serverReceiveThread = new ServerReceiveSocket_Thread(this);
						serverReceiveThread.start();
						
						serverSendThread = new ServerSendSocket_Thread(this);
						serverSendThread.start();
					}
					
				} catch (IOException e) {
					// TODO Auto-generated catch block
					Toast.makeText(this, "Error of create", Toast.LENGTH_SHORT).show();
				}
			}else {
				if (mSendSocket == null && mReceiveSocket == null){
					mSendSocket = new Socket();
					mReceiveSocket = new Socket();
						
					Runnable runnable = new Runnable() {

						@Override
						public void run() {
							// TODO Auto-generated method stub
							try {
								mReceiveSocket.bind(null);
								mReceiveSocket.connect(new InetSocketAddress(hostIP, ServerSendSocket_Thread.PORT), SOCKET_TIMEOUT);
								
								mSendSocket.bind(null);
								mSendSocket.connect(new InetSocketAddress(hostIP, ServerReceiveSocket_Thread.PORT), SOCKET_TIMEOUT);
							} catch (IOException e) {
								// TODO Auto-generated catch block
								e.printStackTrace();
							}

						}
					};
					Thread connect = new Thread(runnable);
					connect.start();
				}
				tvConnectInfo.setText("Group Owner: NO - Host IP: " + hostIP);
			}
		}
	}

	@Override
	public void changeState() {
		// TODO Auto-generated method stub
		startRegistration();
	}
	
	public class DeviceListAdapter extends ArrayAdapter<WifiP2pDevice>{

		Context mContext;
		int mResource;
		List <WifiP2pDevice> mList;
		
		public DeviceListAdapter(Context context, int resource,
				List<WifiP2pDevice> objects) {
			super(context, resource, objects);
			// TODO Auto-generated constructor stub
			
			mContext = context;
			mResource = resource;
			mList = objects;
		}
		
		@Override
		public View getView(int position, View convertView, ViewGroup parent) {
			// TODO Auto-generated method stub
			
			View v = convertView;
			
			LayoutInflater inflater = getLayoutInflater();
			v = inflater.inflate(mResource, null);
			
			TextView tvDeviceName = (TextView)v.findViewById(R.id.tvDeviceName);
			tvDeviceName.setText(mList.get(position).deviceName);
			
			return v; 
		}
		
	}

	@Override
	public void onReceiveData(String imagePath) {
		Bitmap bm = BitmapFactory.decodeFile(imagePath);
		imvReceiveImage.setImageBitmap(bm);	
	}

	@Override
	public void onReceive_SendSocket(Socket sendSocket) {
		// TODO Auto-generated method stub
		mSendSocket = sendSocket;
	}

	@Override
	public void onReceive_ReceiveSocket(Socket receiveSocket) {
		// TODO Auto-generated method stub
		mReceiveSocket = receiveSocket;
		ReceiveSocketAsync receiveThread = new ReceiveSocketAsync(this, receiveSocket);
		receiveThread.start();
	}
	
}
