﻿using System;
using ZqUtils.Extensions;
using ZqUtils.WeChat.Interfaces;

namespace ZqUtils.WeChat.Models
{
    /// <summary>
    /// 视频消息
    /// </summary>
    public class VideoXmlMsg : IXmlMsg
    {
        /// <summary>
        /// 接收方帐号(收到的OpenID)
        /// </summary>
        public string ToUserName { get; set; }
        
        /// <summary>
        /// 开发者微信号
        /// </summary>
        public string FromUserName { get; set; }
        
        /// <summary>
        /// 消息创建时间(整型)
        /// </summary>
        public int CreateTime { get; set; } = DateTime.Now.ToUnixTime();
        
        /// <summary>
        /// 消息类型
        /// </summary>
        public string MsgType { get; set; } = "video";
        
        /// <summary>
        /// 多媒体Id(通过素材管理接口上传多媒体文件，得到的id)
        /// </summary>
        public string MediaId { get; set; }
        
        /// <summary>
        /// 视频消息的标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 视频消息的描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 转换成xml
        /// </summary>
        /// <returns>string</returns>
        public string ToXml()
        {
            return $@"<xml>
                        <ToUserName><![CDATA[{ToUserName}]]></ToUserName>
                        <FromUserName><![CDATA[{FromUserName}]]></FromUserName>
                        <CreateTime>{CreateTime}</CreateTime>
                        <MsgType><![CDATA[{MsgType}]]></MsgType>
                        <Video>
                            <MediaId><![CDATA[{MediaId}]]></MediaId>
                            <Title><![CDATA[{Title}]]></Title>
                            <Description><![CDATA[{Description}]]></Description>
                        </Video> 
                    </xml>";
        }
    }
}