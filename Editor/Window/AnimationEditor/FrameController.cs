using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class FrameController
{
    public delegate void IntEvent(int frame);

    public int currentFrame = 0;
    public int totalFrames = 0;
    public IntEvent onFrameChange = null;
    public List<int> keyframes = new();

    public float lastFrameTime = 0;

    float frame_delta = 1f/60f;

    public void PreviousFrame()
    {
        int frame = currentFrame - 1;
        if (frame < 0) frame = totalFrames;

        SetFrame(frame);
    }

    public void NextFrame()
    {
        int frame = currentFrame + 1;
        if (frame > totalFrames) frame = 0;

        SetFrame(frame);
    }

    public void PreviousKey()
    {
        if (keyframes.Count == 1)
        {
            SetFrame(keyframes[0]);
            return;
        }
        for (int i = 0; i < keyframes.Count; i++)
        {
            if (currentFrame == keyframes[i])
            {
                if (i == 0)
                {
                    SetFrame(keyframes[keyframes.Count - 1]);
                    return;
                }
                else
                {
                    SetFrame(keyframes[i - 1]);
                    return;
                }
            }
            if (i == keyframes.Count - 1)
            {
                SetFrame(keyframes[i - 1]);
                return;
            }
            if (currentFrame > keyframes[i] && currentFrame < keyframes[i + 1])
            {
                SetFrame(keyframes[i]);
                return;
            }
        }
    }

    public void NextKey()
    {
        if (keyframes.Count == 1)
        {
            SetFrame(keyframes[0]);
            return;
        }
        for (int i = 0; i < keyframes.Count; i++)
        {
            if (currentFrame == keyframes[i])
            {
                if (i == keyframes.Count - 1)
                {
                    SetFrame(keyframes[0]);
                    return;
                }
                else
                {
                    SetFrame(keyframes[i + 1]);
                    return;
                }
            }
            if (i == keyframes.Count - 1)
            {
                SetFrame(keyframes[0]);
                return;
            }
            if (currentFrame > keyframes[i] && currentFrame < keyframes[i + 1])
            {
                SetFrame(keyframes[i + 1]);
                return;
            }
        }
    }

    public void PlayAnimation()
    {
        if (Time.realtimeSinceStartup - lastFrameTime >= frame_delta)
        {
            if (currentFrame > totalFrames)
            {
                SetFrame(0);
            }
            else
            {
                SetFrame(currentFrame + (int)(1000f / 30f));
            }

            lastFrameTime = Time.realtimeSinceStartup;
        }
    }

    public void SetFrameRate(float frameRate)
	{
        frame_delta = 1 / frameRate;
	}

    public void SetFrame(int newFrame)
    {
        if (newFrame < 0)
        {
            return;
        }
        currentFrame = newFrame;

        onFrameChange?.Invoke(currentFrame);
    }
}
