using Crestron.SimplSharp;
using Independentsoft.Exchange;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Yoodar
{
    public class YoodarMusic
    {
        private ushort _ChannelID;
        public delegate void DelegateInt(ushort value);
        public delegate void DelegateData(SimplSharpString data);
        public delegate void DelegateString(ushort index, SimplSharpString data);
        public delegate void DelegateIndexString(ushort index,SimplSharpString data);

        public DelegateInt DelegateIntValue { get; set; }
        public DelegateData DelegateTx { get; set; }       
        public DelegateString DelegateStringData { get; set; }
        public DelegateIndexString DelegateIndexStringData { get; set; }

        public DelegateInt DelegateBass { get; set; }
        public DelegateInt DelegateDuration { get; set; }
        public DelegateInt DelegateMute { get; set; }
        public DelegateInt DelegatePlayMode { get; set; }
        public DelegateInt DelegatePlayTime { get; set; }
        public DelegateInt DelegateSource { get; set; }
        public DelegateInt DelegateState { get; set; }
        public DelegateInt DelegateTreb { get; set; }
        public DelegateInt DelegateVolume { get; set; }
        //public DelegateData DelegateAlbum { get; set; }
        //public DelegateData DelegateArtist { get; set; }
        //public DelegateData DelegatePicUrl { get; set; }
        public DelegateData DelegateName { get; set; }
        //public DelegateData Delegate_Duration { get; set; }
        //public DelegateData Delegate_PlayTime { get; set; }

        List<string> CloudIDList = new List<string>();

        public void init(ushort channelID)
        {
            _ChannelID = channelID;
        }
        //云音乐
        private ushort cloudTotal;
        public ushort CloudTotal
        {
            get { return cloudTotal; }
            set { cloudTotal = value; } 
        }

        public ushort CloudBegin { get; set; }
        public string CloudAlbumId { get; set; }
        public ushort CloudType { get; set; }
        public string CloudName { get; set; }
        public string CloudId { get; set; }
        public string CloudPicUrl { get; set; }
        //本地
        public ushort localTotal;
        public ushort LocalTotal
        {
            get
            {
                return localTotal;
            }
            set { localTotal = value; }
        }
        //public ushort LocalTotal { get => localTotal; set => localTotal = value; }
        public string LocalAlbumId { get; set; }
        public ushort LocalType { get; set; }
        public string LocalName { get; set; }
        //播放列表
        public ushort mediaTotal;
        public ushort MediaTotal { get => mediaTotal; set => mediaTotal = value; }
        public ushort MediaBegin { get; set; }
        public string MediaName { get; set; }
        List<string> MediaIDList = new List<string>();


        //播放信息
        public ushort bass { get; set; }//低音0-15
        public string duration { get; set; }//歌曲总时间
        public ushort mute { get; set; }
        public ushort playMode { get; set; }//循环 0x00, 随机 0x01,单曲 0x02
        public string _playTime { get; set; }//当前播放时间
        public ushort source { get; set; }//音源{0：本地；1：FM;2：AUX2;3:NET_RADIO网络电台；4：AUX1;5:云音乐；6：AIRPLAY IOS；7：DLNA IOS/ANDROID}
        public ushort state { get; set; }//当前播放状态 0 停止播放/关闭, 1 打开中, 2 缓冲，3 播放, 4 暂停, 5 准备播放, 0xfe 错误
        public ushort treb { get; set; }//高音0-15
        public ushort volume { get; set; }//0~255
        public string name { get; set; }
        public string album { get; set; }
        public string artist { get; set; }
        public string picUrl { get; set; }
        
        bool flag;


        public void InRx(string rx)
        {
            string str = Substring(rx, "{", "}");
            infoFb(str);
        }

        public void infoFb(string jsonText)
        {
            //云音乐列表
            if (jsonText.StartsWith("{\"ack\":\"list.dirNodeList\""))
            {
                JObject keyValuePairs =(JObject)JsonConvert.DeserializeObject(jsonText);
                CloudTotal = ushort.Parse(keyValuePairs["arg"]["total"].ToString());
                CloudBegin = ushort.Parse(keyValuePairs["arg"]["begin"].ToString());                
                CloudAlbumId = keyValuePairs["arg"]["id"].ToString();
                DelegateIntValue(1);
                string nodeList = keyValuePairs["arg"]["nodeList"].ToString();
                JArray jar = (JArray)JsonConvert.DeserializeObject(nodeList);
                for (int i = 0; i < jar.Count; i++)
                {
                    //JObject j = (JObject)jar[i];
                    JObject j = JObject.Parse(jar[i].ToString());
                    CloudName = StringToUnicode(j["name"].ToString());
                    CloudId = j["id"].ToString();
                    CloudType = (ushort)j["type"];
                    DelegateIntValue(2);
                    ushort ID = (ushort)(i + CloudBegin);
                    ushort index;
                    CloudIDList.Insert(ID, CloudId);
                    if (ID >=0 && ID <=99)
                    {
                        index = (ushort)ID;
                    }
                    else 
                    {
                        index = 99;
                    }
                    var jToken = j["picUrl"];
                    if (jToken != null)
                    {
                        CloudPicUrl = j["picUrl"].ToString();
                        DelegateIndexStringData(index, CloudPicUrl);
                    }
                    DelegateIndexStringData(index, CloudName);
                    //DelegateIndexStringData(index, CloudId);
                }
            }
            //本地文件夹
            //if (jsonText.StartsWith("{\"ack\":\"system.dirNodeList\""))
            //{
            //    JObject keyValuePairs = (JObject)JsonConvert.DeserializeObject(jsonText);
            //    LocalTotal

            //}
            //播放列表
            if (jsonText.StartsWith("{\"ack\":\"list.mediaList\""))
            {
                JObject keyValuePairs = (JObject)JsonConvert.DeserializeObject(jsonText);
                MediaTotal = ushort.Parse(keyValuePairs["arg"]["total"].ToString());
                MediaBegin = ushort.Parse(keyValuePairs["arg"]["begin"].ToString());
                DelegateIntValue(3);
                string nodeList = keyValuePairs["arg"]["nodeList"].ToString();
                JArray jar = (JArray)JsonConvert.DeserializeObject(nodeList);
                for (int i = 0; i < jar.Count; i++)
                {
                    JObject j = JObject.Parse(jar[i].ToString());
                    MediaName = StringToUnicode(j["name"].ToString());
                    string id = j["id"].ToString();
                    ushort index = (ushort)(i + MediaBegin);
                    MediaIDList.Insert(index, id);
                    DelegateIndexStringData(index, MediaName);
                }
            }
            //状态反馈
            if (jsonText.StartsWith("{\"arg\":"))
            {
                try
                {
                    RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(jsonText);                  
                    if (rootObject != null)
                    {                       
                        if (rootObject.arg.volume != null)
                        {
                            DelegateVolume(ushort.Parse(rootObject.arg.volume));
                            //volume = ushort.Parse(rootObject.arg.volume);
                            //DelegateVolume(volume);
                        }
                        if (rootObject.arg.bass != null)
                        {
                            DelegateBass(ushort.Parse(rootObject.arg.bass));
                            //bass = ushort.Parse(rootObject.arg.bass);
                            //DelegateBass(bass);
                        }
                        if (rootObject.arg.duration != null)
                        {
                            //DelegateDuration(ushort.Parse(rootObject.arg.duration));
                            duration = DateTime(rootObject.arg.duration);
                            // Delegate_Duration(duration);
                        }
                        if (rootObject.arg.playTime != null)
                        {
                            //DelegatePlayTime(ushort.Parse(rootObject.arg.playTime));
                            _playTime = DateTime(rootObject.arg.playTime);
                            //Delegate_PlayTime(playTime);
                        }
                        if (rootObject.arg.source != null)
                        {
                            DelegateSource(ushort.Parse(rootObject.arg.source));
                            //source = ushort.Parse(rootObject.arg.source);
                            //DelegateSource(source);
                        }
                        if (rootObject.arg.state != null)
                        {
                            DelegateState(ushort.Parse(rootObject.arg.state));
                            //state = ushort.Parse(rootObject.arg.state);
                            //DelegateState(state);
                        }
                        if (rootObject.arg.treb != null)
                        {
                            DelegateTreb(ushort.Parse(rootObject.arg.treb));
                            //treb = ushort.Parse(rootObject.arg.treb);
                            //DelegateTreb(treb);
                        }
                        if (rootObject.arg.mute != null)
                        {
                            DelegateMute(ushort.Parse(rootObject.arg.mute));
                            //mute = ushort.Parse(rootObject.arg.mute);
                            //DelegateMute(mute);
                        }
                        if (rootObject.arg.playMode != null || rootObject.arg.mode != null)
                        {
                            DelegatePlayMode(ushort.Parse(rootObject.arg.playMode));
                            //playMode = ushort.Parse(rootObject.arg.playMode);
                            //DelegatePlayMode(playMode);
                        }                       
                    }
                    //name = StringToUnicode(rootObject.arg.name);
                    name = rootObject.arg.name;
                    DelegateName(name);
                    //album = StringToUnicode(rootObject.arg.album);
                    album = rootObject.arg.album;
                    //artist = StringToUnicode(rootObject.arg.artist);
                    artist = rootObject.arg.artist;
                    picUrl = rootObject.arg.picUrl;
                    DelegateIntValue(0);
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine(e.Message);
                }
            }
        }



        public void headData(string jsonText)
        {
            int textLenght = jsonText.Length + 5;
            byte[] Lenght = new byte[2];
            if (textLenght > 0)
            {
                Lenght[0] = (byte)(textLenght >> 8);
                Lenght[1] = (byte)textLenght;
            }
            byte[] SendData = new byte[textLenght];
            SendData[0] = 0x0F;
            SendData[1] = (byte)_ChannelID;
            SendData[2] = Lenght[0];
            SendData[3] = Lenght[1];

            for (int i = 4; i < SendData.Length - 1; i++)
            {
                SendData[i] = (byte)jsonText[i - 4];
            }
            byte[] bcc = BCC(SendData);
            SimplSharpString tx = new SimplSharpString();
            //((char)t1).ToString()
            tx = "\x0F" + (char)_ChannelID + (char)Lenght[0] + (char)Lenght[1] + jsonText + (char)bcc[0];
            DelegateTx(tx);
        }
        //BCC校验
        public static byte[] BCC(byte[] data)
        {
            int temp = 0;
            for (int index = 0; index < data.Length; index++)
            {
                temp = temp ^ data[index];
            }

            byte[] result = new byte[1];

            result[0] = Convert.ToByte(temp);

            return result;
        }
        public static string StringToUnicode(string source)
        {
            var bytes = Encoding.Unicode.GetBytes(source);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("&#x{0:x2}{1:x2};", bytes[i + 1], bytes[i]);
            }
            return stringBuilder.ToString();
        }
        public string Substring(string text, string start, string end)
        {
            try
            {
                int IndexofA = text.IndexOf(start);
                int IndexofB = text.LastIndexOf(end);
                string NameText = "";
                if (IndexofA > 0 && IndexofB > 0)
                {
                    NameText = text.Substring(IndexofA, IndexofB + end.Length - IndexofA);
                }
                return NameText;
            }
            catch (Exception e)
            {
                throw;
                CrestronConsole.PrintLine(e.Message);
            }
        }
        private string DateTime(string duration)
        {
            TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(duration));
            string str = "";
            if (ts.Hours == 0 && ts.Minutes > 0)
            {
                //str = ts.Minutes + ": " + ts.Seconds;
                str = String.Format("{0:00}", ts.Minutes) + ":" + String.Format("{0:00}", ts.Seconds);
            }
            if (ts.Hours == 0 && ts.Minutes == 0)
            {
                //str = "00:" + ts.Seconds;
                str = "00:" + String.Format("{0:00}", ts.Seconds);
            }
            return str;
        }



        public void PowerOn()
        {
            //string jsonText = "{\"call\":\"channel.open\"}";
            headData("{\"call\":\"channel.open\"}");
        }
        public void PowerOff()
        {
            headData("{\"call\":\"channel.close\"}");
        }
        public void setSource_Cloud()
        {
            headData("{\"call\":\"player.setSource\", \"arg\":{\"source\": 5}}");
        }
        public void setSource_Local()
        {
            headData("{\"call\":\"player.setSource\", \"arg\":{\"source\": 0}}");
        }
        public void setSource_FM()
        {
            headData("{\"call\":\"player.setSource\", \"arg\":{\"source\": 1}}");
        }
        public void setSource_AUX2()
        {
            headData("{\"call\":\"player.setSource\", \"arg\":{\"source\": 2}}");
        }
        public void setSource_NetRadio()
        {
            headData("{\"call\":\"player.setSource\", \"arg\":{\"source\": 3}}");
        }
        public void setSource_AUX1()
        {
            headData("{\"call\":\"player.setSource\", \"arg\":{\"source\": 4}}");
        }
        public void playPrev()//上一曲
        {
            headData("{\"call\":\"player.playPrev\"}");
        }
        public void playNext()//下一曲
        {
            headData("{\"call\":\"player.playNext\"}");
        }
        public void pause()//暂停
        {
            headData("{\"call\":\"player.pause\"}");
        }
        public void resume()//继续播放
        {
            headData("{\"call\":\"player.resume\"}");
        }
        public void playNextAlbum()//播放下一专辑
        {
            headData("{\"call\":\"player.playNextAlbum\"}");
        }
        public void playPrevAlbum()//播放上一专辑
        {
            headData("{\"call\":\"player.playPrevAlbum\"}");
        }
        public void PlayMode_Repeat()//循环 0x00, 随机 0x01,单曲 0x02
        {
            headData("{\"call\":\"player.setPlayMode\", \"arg\" : {\"mode\":0}}");
        }
        public void PlayMode_Shuffle()//循环 0x00, 随机 0x01,单曲 0x02
        {
            headData("{\"call\":\"player.setPlayMode\", \"arg\" : {\"mode\":1}}");
        }
        public void PlayMode_Singles()//循环 0x00, 随机 0x01,单曲 0x02
        {
            headData("{\"call\":\"player.setPlayMode\", \"arg\" : {\"mode\":2}}");
        }
        public void addVolume() //0 ~ 255,可选, 默认 8
        {
            headData("{\"call\":\"player.addVolume\"}");
        }
        public void decVolume() //或{"call":"player.addVolume" , "arg" : {"stepValue":8}}
        {
            headData("{\"call\":\"player.decVolume\"}");
        }
        public void setMute(ushort value) //{"call":"player.setMute", "arg":{"mute": false}}
        {
            if (value == 1)
            {
                headData("{\"call\":\"player.setMute\", \"arg\":{\"mute\": true}}");
            }
            else
            {
                headData("{\"call\":\"player.setMute\", \"arg\":{\"mute\": false}}");
            }
        }
        public void setVolume(ushort value) //{"call":"player.setVolume","arg":{"volume": 10}}
        {
            if (value >= 0 && value <= 255)
            {
                string Text = "{\"call\":\"player.setVolume\",\"arg\":{\"volume\":" + value + "}}";
                headData(Text);
            }
        }
        public void PlayTime(ushort value) //{"call":"player.seek","arg":{"time": 54}}
        {
            if (value >= 0 && value <= 1000)
            {
                string Text = "{\"call\":\"player.seek\",\"arg\":{\"time\":" + value + "}}";
                headData(Text);
            }
        }
        public void setBass(ushort value) //{"call":"player.setTreb", "arg" : {"treb":0 //0 ~ 15}}
        {
            if (value >= 0 && value <= 15)//0 ~ 15
            {
                string Text = "{\"call\":\"player.setTreb\",\"arg\":{\"bass\":" + value + "}}";
                headData(Text);
            }
        }
        public void setTreb(ushort value) //{"call":"player.setTreb", "arg" : {"treb":0 //0 ~ 15}}
        {
            if (value >= 0 && value <= 15)//0 ~ 15
            {
                string Text = "{\"call\":\"player.setTreb\",\"arg\":{\"treb\":" + value + "}}";
                headData(Text);
            }
        }
        public void setParty() //{"call":"party.set","arg":{"action":3, "autoOpen":true}}    创建
        {
            headData("{\"call\":\"party.set\",\"arg\":{\"action\":3, \"autoOpen\":true}}");
        }
        public void joinParty() //加入
        {
            headData("{\"call\":\"party.set\",\"arg\":{\"action\":1}}");
        }
        public void exitParty() //退出
        {
            headData("{\"call\":\"party.set\",\"arg\":{\"action\":0}}");
        }
        public void dissolutionParty() //解散
        {
            headData("{\"call\":\"party.set\",\"arg\":{\"action\":2}}");
        }
        public void Tmp_url(string url, string volume) //{"call":"player.play","arg":{"url":"url123", "source":0,"tmp":true, "volume":3, "albumUrl":"/xx/"}}
        {                           //","tmp":true, "volume":    ,}}
            string Text = "{\"call\":\"player.play\",\"arg\":{\"url\":\"" + url + "\",\"tmp\":true, \"volume\":" + volume + "}}";
            headData(Text);
        }
        public void info()//通道信息
        {
            headData("{\"call\":\"channel.info\"}");
        }
        public void CloudNodeList(string id, ushort begin)//分类列表{"call":"list.dirNodeList","arg":{"id":0,"begin":0,"size":8,"source":5}}
        {
            flag = true;
            headData("{\"call\":\"list.dirNodeList\",\"arg\":{\"id\":" + id + ",\"begin\":0,\"size\":5,\"source\":5}}");
            Thread.Sleep(1000);
            flag = false;
            CallCloudNodeList(id, begin);
        }
        public void CallCloudNodeList(string id, ushort begin)//分类列表{"call":"list.dirNodeList","arg":{"id":0,"begin":0,"size":8,"source":5}}
        {
            if (cloudTotal <= 100)
            {
                int num = cloudTotal / 5;
                for (int i = 1; i <= num; i++)
                {
                    string Text = "{\"call\":\"list.dirNodeList\",\"arg\":{\"id\":" + id + ",\"begin\":" + i * 5 + ",\"size\":5,\"source\":5}}";
                    headData(Text);
                    Thread.Sleep(500);
                    if(flag)
                    {
                        return;
                    }
                }
            }
            else
            {
                for (int i = 1; i <= 19; i++)
                {
                    string Text = "{\"call\":\"list.dirNodeList\",\"arg\":{\"id\":" + id + ",\"begin\":" + i * 5 + ",\"size\":5,\"source\":5}}";
                    headData(Text);
                    Thread.Sleep(500);
                    if (flag)
                    {
                        return;
                    }
                }
            }
        }
        public void mediaList(ushort begin)//播放列表{"call":"list.mediaList","arg":{"name":"play","begin":0, "size":16}}
        {
            flag = true;
            string Text = "{\"call\":\"list.mediaList\",\"arg\":{\"name\":\"play\",\"begin\":" + begin + ", \"size\":10}}";
            headData(Text);
            Thread.Sleep(1500);
            flag = false;
            CallMediaList();
        }
        //{"call":"player.play","arg":{"id":134217745}}
        public void playList(string id)//播放id  {"call":"player.play","arg":{"id":134217745}}
        {
            string Text = "{\"call\":\"player.play\",\"arg\":{\"id\":" + id + "}}";
            headData(Text);
        }
        public void playMediaList(ushort index)//播放id  {"call":"player.play","arg":{"id":134217745}}
        {
            string id = MediaIDList[index - 1];
            string Text = "{\"call\":\"player.play\",\"arg\":{\"id\":" + id + "}}";
            headData(Text);
        }
        public void playListAlbumId(string id, string albumId)//播放id  {"call":"player.play","arg":{"id":134217745,"albumId":}}
        {
            //flag = true;
            string Text = "{\"call\":\"player.play\",\"arg\":{\"id\":" + id + ",\"albumId\":" + albumId + "}}";
            headData(Text);
        }
        public void CallMediaList()//自动获取
        {
            if (mediaTotal > 0)
            {
                int num = (ushort)(mediaTotal / 10);
                for (int i = 0; i < num; i++)
                {
                    int begin =(i + 1) * 10;
                    headData("{\"call\":\"list.mediaList\",\"arg\":{\"name\":\"play\",\"begin\":" + begin + ", \"size\":10}}");
                    Thread.Sleep(1000);
                    if (flag)
                    {
                        return;
                    }
                }
            }
        }

        //云列表播放
        public void CloudPlay(ushort index)
        {
            if(CloudType ==1)
            {
                CloudNodeList(CloudIDList[index - 1], 0);
            }
            else
            {
                playListAlbumId(CloudIDList[index-1], CloudAlbumId);
            }
        }

    }

    class RootObject
    {
        public Arg arg { get; set; }
        public string notify { get; set; }
    }

    class Arg
    {

        //public string albumUrl { get; set; }        
        public string bass { get; set; }//低音0-15
                                        //public int download { get; set; }
        public string duration { get; set; }//歌曲总时间
                                            //public int eq { get; set; }
                                            //public string id { get; set; }
                                            //public int like { get; set; }
        public string mute { get; set; }
        public string name { get; set; }
        public string playMode { get; set; }//循环 0x00, 随机 0x01,单曲 0x02
        public string mode { get; set; }//循环 0x00, 随机 0x01,单曲 0x02
        public string playTime { get; set; }//当前播放时间
                                            //public int pos { get; set; }
        public string source { get; set; }//音源{0：本地；1：FM;2：AUX2;3:NET_RADIO网络电台；4：AUX1;5:云音乐；6：AIRPLAY IOS；7：DLNA IOS/ANDROID}
        public string state { get; set; }//当前播放状态 0 停止播放/关闭, 1 打开中, 2 缓冲，3 播放, 4 暂停, 5 准备播放, 0xfe 错误
        public string treb { get; set; }//高音0-15
                                        //public string url { get; set; }
        public string volume { get; set; }//0~255
        public string album { get; set; }
        public string artist { get; set; }
        public string picUrl { get; set; }

    }
}
