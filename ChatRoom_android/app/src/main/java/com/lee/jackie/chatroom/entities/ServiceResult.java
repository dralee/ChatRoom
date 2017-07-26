package com.lee.jackie.chatroom.entities;

/**
 * Created by Jackie on 2017/7/25.
 */
// CreatedBy:  Jackie Lee（天宇遊龍）
public class ServiceResult {
    private boolean success;
    private String message;

    public boolean isSuccess() {
        return success;
    }

    public void setSuccess(boolean success) {
        this.success = success;
    }

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    public ServiceResult(boolean success, String message){
        this.success = success;
        this.message = message;
    }
}
