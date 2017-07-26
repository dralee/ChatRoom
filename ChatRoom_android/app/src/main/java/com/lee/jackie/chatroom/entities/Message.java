package com.lee.jackie.chatroom.entities;

import org.json.JSONException;
import org.json.JSONObject;

/**
 * Created by Jackie on 2017/7/25.
 */

// CreatedBy:  Jackie Lee（天宇遊龍）
public class Message {
    // 上次消息值（客户端将上次服务器返回的值原样返回）
    private long queueID;
    // 发送者
    private long from;
    private String fromName;
    // 接收者0为全部
    private long to;
    private String toName;
    // 消息内容
    private String content;

    public long getQueueID() {
        return queueID;
    }

    public void setQueueID(long queueID) {
        this.queueID = queueID;
    }

    public long getFrom() {
        return from;
    }

    public void setFrom(long from) {
        this.from = from;
    }

    public String getFromName() {
        return fromName;
    }

    public void setFromName(String fromName) {
        this.fromName = fromName;
    }

    public long getTo() {
        return to;
    }

    public void setTo(long to) {
        this.to = to;
    }

    public String getToName() {
        return toName;
    }

    public void setToName(String toName) {
        this.toName = toName;
    }

    public String getContent() {
        return content;
    }

    public void setContent(String content) {
        this.content = content;
    }

    public static Message fromJson(JSONObject jsonObject) throws JSONException {
        if(jsonObject == null){
            return null;
        }
        Message message = new Message();
        message.setQueueID(jsonObject.getLong("queueID"));
        message.setFrom(jsonObject.getLong("from"));
        message.setFromName(jsonObject.getString("fromName"));
        message.setTo(jsonObject.getLong("to"));
        message.setToName(jsonObject.getString("toName"));
        message.setContent(jsonObject.getString("content"));
        return message;
    }
}
