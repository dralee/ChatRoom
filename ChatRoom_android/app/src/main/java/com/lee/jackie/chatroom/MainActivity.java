package com.lee.jackie.chatroom;

import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Color;
import android.net.Uri;
import android.os.AsyncTask;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.lee.jackie.chatroom.entities.Message;
import com.lee.jackie.chatroom.entities.ServiceResult;
import com.lee.jackie.chatroom.entities.User;
import com.lee.jackie.chatroom.svrs.AppInfo;

import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.logging.Handler;
import java.util.logging.LogRecord;
// CreatedBy:  Jackie Lee（天宇遊龍）
// CreatedOn: 2017.07.25
public class MainActivity extends AppCompatActivity {

    TextView tvUserName;
    EditText etMsg, etMsgInfo;
    Button btnSend, btnRefresh, btnLogout;
    Spinner spRecv;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        tvUserName = (TextView) findViewById(R.id.tv_user_name);
        etMsg = (EditText) findViewById(R.id.et_msg);
        etMsgInfo = (EditText) findViewById(R.id.et_msg_info);
        btnSend = (Button) findViewById(R.id.btn_send);
        btnRefresh = (Button) findViewById(R.id.btn_refresh);
        spRecv = (Spinner) findViewById(R.id.sp_recv_name);
        btnLogout = (Button) findViewById(R.id.btn_logout);

        spRecv.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                TextView tv = (TextView) view;
                tv.setTextColor(Color.BLUE);
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {

            }
        });

        AppInfo.initialize(this);

        login();

        send();
        //recvMsg();
        refresh();
        logout();
    }

    private void logout() {
        btnLogout.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                btnLogout.setEnabled(false);
                new AsyncLogoutTask().execute();
            }
        });
    }

    private void login() {
        final Spinner spUser = new Spinner(this);
        //spUser.setBackgroundResource(R.drawable.shape_normal);
        //spUser.setMinimumWidth(400);
        //spUser.setMinimumHeight(50);
        //spUser.setBackgroundColor(Color.LTGRAY);
        spUser.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                TextView tv = (TextView) view;
                tv.setTextColor(Color.BLACK);
                //tv.setGravity(Gravity.CENTER); // 布局位置
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {

            }
        });
        new AsyncUserTask(spUser).execute();

        final AlertDialog d = new AlertDialog.Builder(this)
                .setTitle("请选择登录用户")
                .setIcon(android.R.drawable.ic_dialog_info)
                .setView(spUser)
                .setPositiveButton("登录", null).create();
        d.setOnShowListener(new DialogInterface.OnShowListener() {
            @Override
            public void onShow(DialogInterface dialog) {
                final Button b = d.getButton(AlertDialog.BUTTON_POSITIVE);
                b.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        Item item = (Item) spUser.getSelectedItem();
                        if (item == null) {
                            AppInfo.showToast("请稍等，数据正在请求中...");
                            return;
                        }

                        b.setEnabled(false);
                        new AsyncLoginTask(d).execute(item.getValue(), item.getText());
                    }
                });
            }
        });
        d.show();
    }

    private final class AsyncUserTask extends AsyncTask<String, Integer, List<User>> {
        private Spinner spinner;

        public AsyncUserTask(Spinner sp) {
            spinner = sp;
        }

        @Override
        protected List<User> doInBackground(String... params) {
            return AppInfo.reqUsers();
        }

        @Override
        protected void onPostExecute(List<User> users) {
            super.onPostExecute(users);
            ArrayList<Item> list = new ArrayList<>();
            // 数据
            for (User u : users) {
                list.add(new Item(u.getUserName(), String.valueOf(u.getId())));
            }

            // 适配器
            ArrayAdapter adapter = new ArrayAdapter(MainActivity.this.getApplicationContext(), android.R.layout.simple_spinner_item, list);
            adapter.setDropDownViewResource(android.R.layout.simple_dropdown_item_1line);
            spinner.setAdapter(adapter);
            btnRefresh.setEnabled(true);
        }
    }

    private final class AsyncLoginTask extends AsyncTask<String, Integer, ServiceResult> {
        private AlertDialog dialog;

        public AsyncLoginTask(AlertDialog dialog) {
            this.dialog = dialog;
        }

        @Override
        protected ServiceResult doInBackground(String... params) {
            return AppInfo.reqLogin(params[0], params[1]);
        }

        @Override
        protected void onPostExecute(ServiceResult serviceResult) {
            super.onPostExecute(serviceResult);
            if (serviceResult.isSuccess()) {
                dialog.dismiss();
                tvUserName.setText(AppInfo.getInstance().getUser().getUserName());
                new AsyncUserTask(spRecv).execute();
                recvMsg();
            } else {
                AppInfo.showToast(serviceResult.getMessage());
                Button b = dialog.getButton(AlertDialog.BUTTON_POSITIVE);
                b.setEnabled(true);
            }
        }
    }

    private final class AsyncLogoutTask extends AsyncTask<String, Integer, ServiceResult> {

        @Override
        protected ServiceResult doInBackground(String... params) {
            return AppInfo.reqLogout();
        }

        @Override
        protected void onPostExecute(ServiceResult serviceResult) {
            super.onPostExecute(serviceResult);
            if (serviceResult.isSuccess()) {
                login();
            } else {
                AppInfo.showToast(serviceResult.getMessage());
                btnLogout.setEnabled(true);
            }
        }
    }

    private void recvMsg() {
        if (AppInfo.getInstance().getUser() == null) {
            return;
        }
        //new AsyncReceiveTask().execute();
        new Thread(new Runnable() {
            @Override
            public void run() {
                while (true) {
                    doRecv();
                }
            }
        }).start();
    }

    private void doRecv() {
        Message message = AppInfo.reqRecevie();
        if (message != null) {
            final String msg = String.format("(%d)[%s] to [%s] %s\n", message.getQueueID(), message.getFromName(), message.getToName(), message.getContent());
            operOnUiThread(new Runnable() {
                @Override
                public void run() {
                    //etMsgInfo.append(msg);
                    MainActivity.this.etMsgInfo.append(msg);
                }
            });
        }
    }

    private void refresh() {
        btnRefresh.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                btnRefresh.setEnabled(false);
                new AsyncUserTask(spRecv).execute();
            }
        });
    }

    private void send() {
        btnSend.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Item item = (Item) spRecv.getSelectedItem();
                //Toast.makeText(getApplicationContext(), item.getText()+"_"+item.getValue(), Toast.LENGTH_LONG).show();
                btnSend.setEnabled(false);
                String msg = etMsg.getText().toString();
                new AsyncSendTask().execute(item.getValue(), msg);
            }
        });
    }

    private final class AsyncSendTask extends AsyncTask<String, Integer, String> {

        @Override
        protected String doInBackground(String... params) {
            AppInfo.reqSend(Long.parseLong(params[0]), params[1]);
            return "";
        }

        @Override
        protected void onPostExecute(String s) {
            super.onPostExecute(s);
            btnSend.setEnabled(true);
        }
    }

    private final class AsyncReceiveTask extends AsyncTask<String, Integer, Message> {

        @Override
        protected Message doInBackground(String... params) {
            return AppInfo.reqRecevie();
        }

        @Override
        protected void onPostExecute(Message message) {
            super.onPostExecute(message);
            if (message != null) {
                String msg = String.format("(%d)[%s] to [%s] %s\n", message.getQueueID(), message.getFromName(), message.getToName(), message.getContent());
                etMsgInfo.append(msg);
            }
            recvMsg();
        }
    }

    public void operOnUiThread(Runnable runnable) {
        runOnUiThread(runnable);
    }

    public void makeToastText(String message, int periodTime) {
        if (periodTime != Toast.LENGTH_LONG && periodTime != Toast.LENGTH_SHORT)
            periodTime = Toast.LENGTH_SHORT;
        Toast.makeText(this, message, periodTime).show();
    }

    class Item {
        private String value;
        private String text;

        public Item() {
        }

        public Item(String text, String value) {
            this.value = value;
            this.text = text;
        }

        public String getText() {
            return text;
        }

        public String getValue() {
            return value;
        }

        public String toString() {
            return text;
        }
    }
}
