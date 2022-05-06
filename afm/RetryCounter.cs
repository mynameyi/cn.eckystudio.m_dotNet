using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.AutomationFlowModel
{
    public class RetryCounter
    {
        private int mCurrentTimes = 0;
        private int mCurrentRound = 0;
        List<int[]> mRetryRoundList = new List<int[]>();

        public void Add(int interval,int times)
        {
            int[] round = new int[] { interval, times };
            mRetryRoundList.Add(round);
        }

        public int Retry()
        {
            if (mRetryRoundList.Count != 0)
            {
                for (int i = mCurrentRound; i < mRetryRoundList.Count; i++)
                {
                    int[] iRound = mRetryRoundList[mCurrentRound];
                    if (mCurrentTimes < iRound[1])
                    {
                        mCurrentTimes++;
                        return iRound[0];
                    }
                    else
                    {
                        mCurrentTimes = 0;
                        mCurrentRound++;                       
                    }
                }
            }
            return -1;
        }

        public void Reset()
        {
            mCurrentTimes = 0;
            mCurrentRound = 0;
        }
    }
}
