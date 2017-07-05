#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.Runtime.InteropServices;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Ableton Link", Category = "Value", Version = "", Help = "Sync Timelines using Ableton Link", Tags = "Sync", Author = "soriak")]
    #endregion PluginInfo
    public class SyncAbletonLinkValueNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins
        [Input("BPM In", DefaultValue = 120)]
        public IDiffSpread<int> FbpmIn;

        [Input("Set Tempo", IsBang = true)]
        public IDiffSpread<bool> FSetTempoIn;

        [Input("Reset", IsBang = true)]
        public IDiffSpread<bool> FResetIn;

        [Input("Enabled")]
        public IDiffSpread<bool> FEnabledIn;

        [Output("Beat")]
        public ISpread<double> FBeatOut;

        [Output("Phase")]
        public ISpread<double> FPhaseOut;

        [Output("Session BPM")]
        public ISpread<double> FbpmOut;

        [Output("Peers")]
        public ISpread<int> FPeersOut;


        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        private bool isEnabled = true;

        public void Dispose()
        {
            AbletonLink.Instance.Dispose();
                 
        }

        
        public void Evaluate(int SpreadMax)
        {
            if (FResetIn[0])
                this.Dispose();


            if (isEnabled != FEnabledIn[0])
            {
                AbletonLink.Instance.enable(FEnabledIn[0]);
                isEnabled = FEnabledIn[0];
            }

            if (isEnabled)
            {
                if (FSetTempoIn[0])
                    AbletonLink.Instance.setTempo(FbpmIn[0]);

                double beat;
                double phase;
                AbletonLink.Instance.update(out beat, out phase);

                FBeatOut[0] = beat;
                FPhaseOut[0] = phase;
                
                FPeersOut[0] = AbletonLink.Instance.numPeers();
                FbpmOut[0] = AbletonLink.Instance.tempo();
            }

            




        }
    }

    public class AbletonLink : IDisposable
    {
  
    
	private static volatile AbletonLink singletonInstance;
	private IntPtr nativeInstance = IntPtr.Zero;
	private const double INITIAL_TEMPO = 120.0;

	public static AbletonLink Instance
	{
		get
		{
			if (singletonInstance == null)
			{
				
				singletonInstance = new AbletonLink();               
				singletonInstance.setup(INITIAL_TEMPO);
				
			}
			return singletonInstance;
		}
	}

	
	[DllImport ("AbletonLinkDLL")]
	private static extern IntPtr CreateAbletonLink();
	private AbletonLink()
	{
		nativeInstance = CreateAbletonLink();
	}

	~AbletonLink()
	{
		this.Dispose();
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern void DestroyAbletonLink(IntPtr ptr);
	public void Dispose()
	{
            singletonInstance = null;
            if (nativeInstance != IntPtr.Zero) {
			DestroyAbletonLink(nativeInstance);
			nativeInstance = IntPtr.Zero;
                
		}
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern void setup(IntPtr ptr, double bpm);
    
	private void setup(double bpm)
	{
		setup(nativeInstance, bpm);
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern void setTempo(IntPtr ptr, double bpm);
	public void setTempo(double bpm)
	{
		setTempo(nativeInstance, bpm);
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern double tempo(IntPtr ptr);
	public double tempo()
	{
		return tempo(nativeInstance);
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern void setQuantum(IntPtr ptr, double quantum);
	public void setQuantum(double quantum)
	{
		setQuantum(nativeInstance, quantum);
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern double quantum(IntPtr ptr);
	public double quantum()
	{
		return quantum(nativeInstance);
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern bool isEnabled(IntPtr ptr);
	public bool isEnabled()
	{
		return isEnabled(nativeInstance);
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern void enable(IntPtr ptr, bool bEnable);
	public void enable(bool bEnable)
	{
		enable(nativeInstance, bEnable);
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern int numPeers(IntPtr ptr);
	public int numPeers()
	{
		return numPeers(nativeInstance);
	}

	[DllImport ("AbletonLinkDLL")]
	private static extern void update(IntPtr ptr, out double beat, out double phase);
	public void update(out double beat, out double phase)
	{
		update(nativeInstance, out beat, out phase);
	}
}
}
