using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Manager.UI;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class SoundManager : Singleton<SoundManager>
    {
        /// <summary>
        /// 用来播放背景音乐（循环）
        /// </summary>
        [SerializeField]
        private AudioSource bgmAudioSource;
        /// <summary>
        /// 特效的音乐
        /// </summary>
        [SerializeField]
        private AudioSource effectAuduioSource;
        /// <summary>
        /// 等待的队列
        /// </summary>
        private Queue<AudioClip> acEffectQue = new Queue<AudioClip>();

        void Start()
        {
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = true;

            effectAuduioSource.loop = false;
            effectAuduioSource.playOnAwake = false;

        }


        #region 背景音乐
        /// <summary>
        /// 背景音乐的音量
        /// </summary>
        public float BGVolume
        {
            get { return bgmAudioSource.volume; }
            set { bgmAudioSource.volume = value; }
        }
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clip"></param>
        public void PlayBgMusic(AudioClip clip)
        {
            if (clip == null) return;
            bgmAudioSource.clip = clip;
            bgmAudioSource.Play();
        }
        /// <summary>
        /// 停止背景音乐的播放
        /// </summary>
        public void StopBgMusic()
        {
            bgmAudioSource.clip = null;
            bgmAudioSource.Stop();
        }
        #endregion
        #region 特效音乐
        /// <summary>
        /// 播放特效音乐
        /// </summary>
        /// <param name="clip"></param>
        public void PlayEffectMusice(AudioClip clip)
        {
            //if (clip == null) return;
            //effectAuduioSource.clip = clip;
            //effectAuduioSource.Play();
            acEffectQue.Enqueue(clip);
        }

        void Update()
        {
            //如果 这个as不在播放状态 并且 等待播放的音效文件数量大于0
            if (!effectAuduioSource.isPlaying && acEffectQue.Count > 0)
            {
                //先出对头的一个文件
                AudioClip ac = acEffectQue.Dequeue();
                //开始播放
                effectAuduioSource.clip = ac;
                effectAuduioSource.Play();
            }
        }

        /// <summary>
        /// 停止特效音乐的播放
        /// </summary>
        public void StopEffectMusic()
        {
            effectAuduioSource.clip = null;
            effectAuduioSource.Stop();
        }
        #endregion


    }
}
