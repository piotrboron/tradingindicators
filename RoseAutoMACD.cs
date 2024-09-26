using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MACDCrossoverBot : Robot
    {
        private MacdHistogram _macd;
        private AverageDirectionalMovementIndexRating _adx;
        private Position _currentPosition;

        //Volume
        [Parameter("Volume", DefaultValue = 0.1, Group = "Volume")]
        public double Volume { get; set; }
        
        //Longs/Shorts
        
        [Parameter("Trade Long Positions", DefaultValue = false, Group = "Volume")]
        public bool playLongs { get; set; }
        
        [Parameter("Trade Short Positions", DefaultValue = false, Group = "Volume")]
        public bool playShorts { get; set; }
        
        //TP/SL

        [Parameter("Stop Loss (pips)", DefaultValue = 0, Group = "TP/SL")]
        public double StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 0, Group = "TP/SL")]
        public double TakeProfit { get; set; }
        
        //ADX
        
        [Parameter("ADX Filter Value", DefaultValue = 20, Group = "ADX")]
        public double adxValue { get; set; }
        
        
        //MACD

        [Parameter("MACD Fast EMA", DefaultValue = 12, Group = "MACD")]
        public int FastEMA { get; set; }

        [Parameter("MACD Slow EMA", DefaultValue = 26, Group = "MACD")]
        public int SlowEMA { get; set; }

        [Parameter("Signal Smoothing Period", DefaultValue = 9, Group = "MACD")]
        public int SignalPeriod { get; set; }

        protected override void OnStart()
        {
            _macd = Indicators.MacdHistogram(Bars.ClosePrices, FastEMA, SlowEMA, SignalPeriod);
            _adx = Indicators.AverageDirectionalMovementIndexRating(14);
        }

        protected override void OnBar()
        
        {
            bool adxGood = _adx.ADX.LastValue > adxValue;
        
            if (_macd.Histogram.Last(1) > 0 && _macd.Histogram.Last(2) <= 0 && adxGood && playShorts)
            {
                CloseOppositePosition();
                ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "RoseAuto Short Entry", StopLoss, TakeProfit);
            }
            else if (_macd.Histogram.Last(1) < 0 && _macd.Histogram.Last(2) >= 0 && adxGood && playLongs)
            {
                CloseOppositePosition();
                ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "RoseAuto Long Entry", StopLoss, TakeProfit);
            }
        }

        private void CloseOppositePosition()
        {
            if (_currentPosition != null && _currentPosition.SymbolCode == Symbol.Code)
            {
                if (_currentPosition.TradeType == TradeType.Buy)
                    ClosePosition(_currentPosition);
                else if (_currentPosition.TradeType == TradeType.Sell)
                    ClosePosition(_currentPosition);
            }
        }

        protected override void OnPositionOpened(Position position)
        {
            _currentPosition = position;
        }
    }
}
