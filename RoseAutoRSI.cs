using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RoseAutoRSI : Robot
    {
        private MovingAverage ema20, ema50, ema200;
        private RelativeStrengthIndex rsi;
        
        // Wolumen
        [Parameter("Volume (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }
        
        [Parameter("Max Base positions", Group = "Volume", DefaultValue = 0)]
        public int MaxBasePos { get; set; }

        [Parameter("Max Trend positions", Group = "Volume", DefaultValue = 0)]
        public int MaxPos { get; set; }
        
        [Parameter("Max Retracement positions", Group = "Volume", DefaultValue = 0)]
        public int MaxRePos { get; set; }
        
        // Strategie
        
        [Parameter("Base RSI Strategy", DefaultValue = false, Group = "Strategies")]
        public bool baseStrategy { get; set; }
        
        [Parameter("Trend RSI Strategy", DefaultValue = false, Group = "Strategies")]
        public bool trendRSIStrategy { get; set; }
        
        [Parameter("Retracement RSI Strategy", DefaultValue = false, Group = "Strategies")]
        public bool retracementStrategy { get; set; }
        
        [Parameter("Auto Close", DefaultValue = false, Group = "Strategies")]
        public bool AutoClose { get; set; }
        
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
        
        // RSI
        
        [Parameter("RSI Overbought Level", DefaultValue = 70, Group = "RSI")]
        public int OverboughtLevel { get; set; }

        [Parameter("RSI Oversold Level", DefaultValue = 30, Group = "RSI")]
        public int OversoldLevel { get; set; }
        
        [Parameter("RSI Periods", DefaultValue = 14, MinValue = 5, Group = "RSI")]
        public int RSIPeriods { get; set; }

        protected override void OnStart()
        {
            // Load indicators on start up
            ema20 = Indicators.MovingAverage(SourceSeries, FastEMA, MAType);
            ema50 = Indicators.MovingAverage(SourceSeries, MediumEMA, MAType);
            ema200 = Indicators.MovingAverage(SourceSeries, SlowestEMA, MAType);
            // Load RSI indicator on start up
            rsi = Indicators.RelativeStrengthIndex(Bars.ClosePrices, RSIPeriods);
        }
        
        protected override void OnTick()
        {

        }

protected override void OnBar()
{
    //RSI
    double rsiValue = rsi.Result.LastValue;
    
    //TREND
    bool isUptrend = ema20.Result.Last(1) > ema200.Result.Last(1);
    bool isDowntrend = ema20.Result.Last(1) < ema200.Result.Last(1);
    
    if(baseStrategy)
    {
        if(rsiValue > OverboughtLevel && Positions.Count < MaxBasePos)
        {
            ExecuteMarketOrder(TradeType.Sell, Symbol, Quantity, "Base Sell Order", StopLoss, TakeProfit);
        }
        
        if(rsiValue < OversoldLevel && AutoClose)
        {
            CloseShortPositions();
        }
        
        
        if(rsiValue < OversoldLevel && Positions.Count < MaxBasePos)
        {
            ExecuteMarketOrder(TradeType.Buy, Symbol, Quantity, "Base Buy Order", StopLoss, TakeProfit);
        }
        
        if(rsiValue > OverboughtLevel && AutoClose)
        {
            CloseLongPositions();
        }
    
    }
    
    if(trendRSIStrategy)
    {
        if(rsiValue > OverboughtLevel && isUptrend && Positions.Count < MaxPos)
        {
            ExecuteMarketOrder(TradeType.Buy, Symbol, Quantity, "Trend Buy Order", StopLoss, TakeProfit);
        }
        
        if(isUptrend && ema20.Result.LastValue < ema50.Result.LastValue && AutoClose)
        {
            CloseLongPositions();
        }
        
        
        if(rsiValue < OversoldLevel && isDowntrend && Positions.Count < MaxPos)
        {
            ExecuteMarketOrder(TradeType.Sell, Symbol, Quantity, "Trend Sell Order", StopLoss, TakeProfit);
        }
        
        if(isDowntrend && ema20.Result.LastValue > ema50.Result.LastValue && AutoClose)
        {
            CloseShortPositions();
        }
    
    }
    
    if(retracementStrategy)
    {
            // LONGS
            if (rsi.Result.Last(1) < OversoldLevel && rsi.Result.LastValue > OversoldLevel && Positions.Count < MaxRePos)
            {
                CloseShortPositions();
            ExecuteMarketOrder(TradeType.Buy, Symbol, Quantity, "Retracement Buy Order", StopLoss, TakeProfit);
            }
            
            if (rsi.Result.Last(1) < OversoldLevel && rsi.Result.LastValue > OversoldLevel && AutoClose)
            {
                CloseShortPositions();
            }
            // Shorts
            if (rsi.Result.Last(1) > OverboughtLevel && rsi.Result.LastValue < OverboughtLevel && Positions.Count < MaxRePos)
            {
                CloseLongPositions();
                ExecuteMarketOrder(TradeType.Sell, Symbol, Quantity, "Retracement Sell Order", StopLoss, TakeProfit);
            }
            
            if (rsi.Result.Last(1) > OverboughtLevel && rsi.Result.LastValue < OverboughtLevel && AutoClose)
            {
                CloseLongPositions();
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
