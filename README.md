# RabbitMQ
消息队列

#一、基础命令
安装完成RabbitMQ以后，会有一个默认的账户Guest，这个用户得删除,然后新建一个自己的用户，并且赋予管理员权限

####1.新建用户
cmd->到RabbitMQ的安装目录下的一个Sbin目录,正常情况下安装完成后在开始菜单栏可以找到这个目录
```
rabbitmqctl  add_user  hc 123456
```
![](/RabbitImg/CreateUser.png)
这样就创建了一个username=hc,password=123456的用户了

####2.创建虚拟主机
虚拟主机这个概念,很不好懂，看了别人的博客,可以把这个概念理解成c#里面的“名称空间”
```
rabbitmqctl  add_vhoust  hc_mq
```
这里就创建了一个名称为hc_mq的虚拟主机

![](/RabbitImg/Createvhost.png)

####3.给用户、虚拟主机赋予权限
```
rabbitmqctl  set_permissions  -p hc_mq hc  ".*"  ".*"  ".*"
```
![](/RabbitImg/SettingPermissions2.png)
这里就为hc用户,hc_mq的虚拟主机赋予了所有权限,当然用户跟虚拟主机的权限也可以单独分开来赋权限

####4.设置角色
```
rabbitmqctl  set_user_tags yy administrator
```
![](/RabbitImg/SettingUserTags.png)
这里就给hc用户设置了超级管理员的角色
