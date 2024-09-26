using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RoseAutoMA : Robot
    {
        private MovingAverage ema20, ema50, ema200;
        private AverageDirectionalMovementIndexRating adx;
        
        // Wolumen
        [Parameter("Volume (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("Max positions", Group = "Volume", DefaultValue = 0)]
        public int MaxPos { get; set; }
        
        [Parameter("Max Base positions", Group = "Volume", DefaultValue = 0)]
        public int MaxBasePos { get; set; }
        
        // Strategie
        [Parameter("Base MA Strategy", DefaultValue = false, Group = "Strategies")]
        public bool baseStrategy { get; set; }
        
        [Parameter("MA Crossover Strategy", DefaultValue = false, Group = "Strategies")]
        public bool crossOverMAStrategy { get; set; }
        
        [Parameter("Inverse Strategy", DefaultValue = false, Group = "Strategies")]
        public bool inverseMAStrategy { get; set; }
        
        
        //  TP/SL
        [Parameter("Stop Loss (pips)", DefaultValue = 0, Group = "TP/SL")]
        public double StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 0, Group = "TP/SL")]
        public double TakeProfit { get; set; }
        
        // EMAs
        [Parameter("MA Type", Group = "Moving Average")]
        public MovingAverageType MAType { get; set; }
        
        [Parameter("Source", Group = "Moving Average")]
        public DataSeries SourceSeries { get; set; }
        
        [Parameter("Fast EMA Period", DefaultValue = 20, MinValue = 5, Group = "EMA")]
        public int FastEMA { get; set; }
        
        [Parameter("Medium EMA Period", DefaultValue = 50, MinValue = 5, Group = "EMA")]
        public int MediumEMA { get; set; }
        
        [Parameter("Slowest EMA Period", DefaultValue = 200, MinValue = 5, Group = "EMA")]
        public int SlowestEMA { get; set; }
        
        // ADX
        
        [Parameter("ADX Filter On/Off", DefaultValue = false, Group = "ADX")]
        public bool ADX_On { get; set; }
        
        [Parameter("ADX Periods", DefaultValue = 14, MinValue = 5, Group = "ADX")]
        public int ADXPeriods { get; set; }
        
        [Parameter("ADX MinVal", DefaultValue = 25, MinValue = 5, Group = "ADX")]
        public int ADX_Required { get; set; }

        protected override void OnStart()
        {
            // Load indicators on start up
            ema20 = Indicators.MovingAverage(SourceSeries, FastEMA, MAType);
            ema50 = Indicators.MovingAverage(SourceSeries, MediumEMA, MAType);
            ema200 = Indicators.MovingAverage(SourceSeries, SlowestEMA, MAType);
            adx = Indicators.AverageDirectionalMovementIndexRating(ADXPeriods);
        }
        
        protected override void OnTick()
        {

        }

protected override void OnBar()
{

    bool isUptrend = ema50.Result.Last(1) > ema200.Result.Last(1);
    bool isDowntrend = ema50.Result.Last(1) < ema200.Result.Last(1);

    if (baseStrategy && isUptrend)
    {
        if(Positions.Count < MaxBasePos){
                    ExecuteMarketOrder(TradeType.Buy, Symbol, Quantity, "Base Buy Order", StopLoss, TakeProfit);
        }
        
        CloseShortPositions();
        
    }
    
    if (baseStrategy && isDowntrend)
    {
        if(Positions.Count < MaxBasePos ){
            ExecuteMarketOrder(TradeType.Sell, Symbol, Quantity, "Base Sell Order", StopLoss, TakeProfit);
        }
        
        CloseLongPositions();
        
    }


    if (crossOverMAStrategy && isUptrend)
    {
    
        if(!ADX_On){
        
        if (ema20.Result.HasCrossedAbove(ema50.Result,1) && Positions.Count < MaxPos)
        {
            ExecuteMarketOrder(TradeType.Buy, Symbol, Quantity, "Cross Buy Order", StopLoss, TakeProfit);
            CloseShortPositions();
        }
        
        
        if (ema20.Result.HasCrossedBelow(ema50.Result,1))
        {
            CloseLongPositions();
        }
                }
    
        if(ADX_On){
        
        if (ema20.Result.HasCrossedAbove(ema50.Result,1) && ADX_Required > adx.ADX.LastValue && Positions.Count < MaxPos)
        {
            ExecuteMarketOrder(TradeType.Buy, Symbol, Quantity, "Cross Buy Order", StopLoss, TakeProfit);
            CloseShortPositions();
        }
        
        
        if (ema20.Result.HasCrossedBelow(ema50.Result,1) && ADX_Required > adx.ADX.LastValue)
        {
            CloseLongPositions();
        }
                }
        
        
    }

    if (crossOverMAStrategy && isDowntrend)
    {
        if(!ADX_On){
        
        if (ema20.Result.HasCrossedBelow(ema50.Result,1) && Positions.Count < MaxPos)
        {
            ExecuteMarketOrder(TradeType.Sell, Symbol, Quantity, "Cross Sell Order", StopLoss, TakeProfit);
            CloseLongPositions();
        }
        
        
        if (ema20.Result.HasCrossedAbove(ema50.Result,1))
        {
            CloseShortPositions();
        }
        
                }
    
        if(ADX_On){
        
        if (ema20.Result.HasCrossedBelow(ema50.Result,1) && ADX_Required > adx.ADX.LastValue && Positions.Count < MaxPos)
        {
            ExecuteMarketOrder(TradeType.Sell, Symbol, Quantity, "Cross Sell Order", StopLoss, TakeProfit);
            CloseLongPositions();
        }
        
        
        if (ema20.Result.HasCrossedAbove(ema50.Result,1) && ADX_Required > adx.ADX.LastValue)
        {
            CloseShortPositions();
        }
        
                }
        }

}


 
private void CloseShortPositions()
{
    foreach (var position in Positions)
    {
        if (position.TradeType == TradeType.Sell)
        {
            ClosePosition(position);
        }
    }   
}

private void CloseLongPositions()
{
    foreach (var bposition in Positions)
    {
        if (bposition.TradeType == TradeType.Buy)
        {
            ClosePosition(bposition);
        }
    }   
}
 
 
}
    }
