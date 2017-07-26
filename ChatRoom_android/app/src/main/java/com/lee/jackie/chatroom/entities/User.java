package com.lee.jackie.chatroom.entities;

/**
 * Created by Jackie on 2017/7/25.
 */
// CreatedBy:  Jackie Lee（天宇遊龍）
public class User{
    private long id;
    private String userName;
    public long getId() { return id; }
    public String getUserName() { return userName; }

    public User(){}
    public User(long id, String userName){
        this.id=id;
        this.userName = userName;
    }
}
