package com.lee.jackie.chatroom.svrs;

import android.support.annotation.NonNull;

import com.lee.jackie.chatroom.MainActivity;

import java.io.BufferedInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.PrintWriter;
import java.io.UnsupportedEncodingException;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;

import com.lee.jackie.chatroom.R;
import com.lee.jackie.chatroom.entities.Message;
import com.lee.jackie.chatroom.entities.ServiceResult;
import com.lee.jackie.chatroom.entities.User;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

/**
 * Created by Jackie on 2017/7/25.
 */
// CreatedBy:  Jackie Lee（天宇遊龍）
public class AppInfo {
    private static AppInfo appInfo;
    private String serverIp;
    private User user;

    private AppInfo() {
    }

    private MainActivity mainActivity;
    private static long queueId;
    private final static String urlLogin = "/Message/Login";
    private final static String urlLogout = "/Message/Logout?userId=%d";
    private final static String urlSend = "/Message/Send";
    private final static String urlReceive = "/Message/Receive?userId=%d&queueId=%d";

    public static void initialize(MainActivity mainActivity) {
        appInfo = new AppInfo();
        appInfo.mainActivity = mainActivity;
        appInfo.serverIp = mainActivity.getString(R.string.server_ip);
    }

    public static AppInfo getInstance() {
        return appInfo;
    }

    public void setUser(User user) {
        this.user = user;
    }

    public User getUser() {
        return user;
    }

    public static void showToast(String msg) {
        appInfo.mainActivity.makeToastText(msg, 200);
    }

    public static void showToastSub(final String msg) {
        appInfo.mainActivity.operOnUiThread(new Runnable() {
            @Override
            public void run() {
                showToast(msg);
            }
        });
    }

    public static List<User> reqUsers() {
        String path = "/Message/GetUsers?isAll=" + (appInfo.user == null);
        String url = "http://" + appInfo.serverIp + path;
        byte[] buffer = postUrl(url, "");

        if (buffer == null) {
            return null;
        }

        try {
            String res = new String(buffer, "utf-8");
            JSONArray jArray = new JSONArray(res);
            List<User> users = new ArrayList<>();
            if (jArray != null && jArray.length() > 0) {
                for (int i = 0; i < jArray.length(); ++i) {
                    JSONObject jsonObject = jArray.getJSONObject(i);
                    User u = new User(jsonObject.getLong("id"), jsonObject.getString("userName"));
                    users.add(u);
                }
            }
            return users;
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
        } catch (JSONException e) {
            e.printStackTrace();
        }
        return null;
    }

    public static ServiceResult reqLogin(String id, String userName) {
        String url = "http://" + appInfo.serverIp + urlLogin;
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("Id", id);
            jsonObject.put("UserName", userName);
            ServiceResult sr = reqService(url, jsonObject.toString());//"Id="+id+"&UserName="+userName);
            if (sr.isSuccess()) {
                appInfo.setUser(new User(Long.parseLong(id), userName));
            }
            return sr;
        } catch (JSONException e) {
            e.printStackTrace();
        }
        return new ServiceResult(false, "请求接口参数异常");
    }

    public static ServiceResult reqLogout() {
        String url = "http://" + appInfo.serverIp + String.format(urlLogout,appInfo.getUser().getId());
        ServiceResult sr = reqService(url, "");
        if(sr.isSuccess()){
            appInfo.setUser(null);
        }
        return sr;
    }

    public static ServiceResult reqService(String url, String parameter) {
        byte[] buffer = postUrl(url, parameter);
        if (buffer == null) {
            return new ServiceResult(false, "网络请求失败");
        }
        try {
            String res = new String(buffer, "utf-8");
            JSONObject jsonObject = new JSONObject(res);
            return new ServiceResult(jsonObject.getBoolean("success"), jsonObject.getString("message"));
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
        } catch (JSONException e) {
            e.printStackTrace();
        }
        return new ServiceResult(false, "网络请求数据异常");
    }

    public static void reqSend(long userId, String message) {
        String url = "http://" + appInfo.serverIp + urlSend;

        try {
            JSONObject jsonObject = new JSONObject();
            jsonObject.put("queueID", queueId);
            jsonObject.put("from", appInfo.getUser().getId());
            jsonObject.put("to", userId);
            jsonObject.put("content", message);

            postUrl(url, jsonObject.toString());

        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    public static Message reqRecevie() {
        String path = String.format(urlReceive, appInfo.getUser().getId(), queueId);
        String url = "http://" + appInfo.serverIp + path;
        byte[] buffer = postUrl(url, "");

        if (buffer == null) {
            return null;
        }

        try {
            String res = new String(buffer, "utf-8");
            JSONObject jsonObject = new JSONObject(res);
            Message message = Message.fromJson(jsonObject);
            if(message != null){
                queueId = message.getQueueID();
            }
            return message;
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
        } catch (JSONException e) {
            e.printStackTrace();
        }
        return null;
    }

    public static byte[] postUrl(String strUrl, String parameter) {
        if (strUrl == null || strUrl.isEmpty()) {
            return null;
        }
        try {
            URL url = new URL(strUrl);
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.setRequestMethod("POST");
            conn.setDoOutput(true);
            conn.setDoInput(true);

            if (parameter != null && !parameter.isEmpty()) {
                conn.setRequestProperty("Content-Type", "application/json");
                PrintWriter printWriter = new PrintWriter(conn.getOutputStream());
                printWriter.write(parameter);
                printWriter.flush();
                printWriter.close();
            }

            BufferedInputStream bis = new BufferedInputStream(conn.getInputStream());
            ByteArrayOutputStream bos = new ByteArrayOutputStream();
            int len;
            byte[] buffer = new byte[1024];
            while ((len = bis.read(buffer)) != -1) {
                bos.write(buffer, 0, len);
                bos.flush();
            }

            byte[] res = bos.toByteArray();
            bos.close();
            return res;
        } catch (MalformedURLException e) {
            showToastSub("MalformedURLException:" + e.getMessage());
            e.printStackTrace();
        } catch (IOException e) {
            showToastSub("IOException:" + e.getMessage());
            e.printStackTrace();
        }
        return null;
    }
}
