using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MACDCrossoverBot : Robot
    {
        private MacdCrossOver _macd;
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

        [Parameter("MACD Longcycle", DefaultValue = 26, Group = "MACD")]
        public int LongCycle { get; set; }
        
        [Parameter("MACD Shortcycle", DefaultValue = 12, Group = "MACD")]
        public int ShortCycle { get; set; }
        
        [Parameter("MACD Signal Periods", DefaultValue = 9, Group = "MACD")]
        public int MacdSignalPeriods { get; set; }

        protected override void OnStart()
        {
            _macd = Indicators.MacdCrossOver(LongCycle, ShortCycle, MacdSignalPeriods);
            _adx = Indicators.AverageDirectionalMovementIndexRating(14);
        }

        protected override void OnBar()
        {
            bool adxGood = _adx.ADX.LastValue > adxValue;

            if (_macd.Signal.HasCrossedAbove(_macd.MACD,1) && adxGood && playShorts)
            {
                CloseOppositePosition();
                ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "MACD Short Entry", StopLoss, TakeProfit);
            }
            else if (_macd.Signal.HasCrossedBelow(_macd.MACD,1) && adxGood && playLongs)
            {
                CloseOppositePosition();
                ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "MACD Long Entry", StopLoss, TakeProfit);
            }
        }

        private void CloseOppositePosition()
        {
            if (_currentPosition != null && _currentPosition.SymbolCode == Symbol.Code)
            {
                ClosePosition(_currentPosition);
            }
        }

        protected override void OnPositionOpened(Position position)
        {
            _currentPosition = position;
        }
    }
}
