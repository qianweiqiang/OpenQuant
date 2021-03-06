﻿using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using OpenQuant.API.Plugins;

namespace QuantBox.OQ.Demo.Indicator
{
    /// <summary>
    /// Kaufman自适应移动平均
    /// </summary>
    public class KaufmanAMA : UserIndicator
    {
        int n, p, q;
        public TimeSeries ER = new TimeSeries("ER");

        public KaufmanAMA(ISeries series, int n, int p, int q)
            : base(series)
        {
            this.n = n;
            this.p = p;
            this.q = q;
            this.Name = string.Format("KaufmanAMA({0},{1},{2})", n, p, q);
        }

        public override double Calculate(int index)
        {
            if (index - n < 0)
                return double.NaN;

            DateTime dt = Input.GetDateTime(index);

            double DIRECTION = Math.Abs(Input[index, BarData.Close] - Input[index - n, BarData.Close]);

            double VOLATILITY = 0;
            for (int i = index - n; i < index; ++i)
            {
                VOLATILITY += Math.Abs(Input[i + 1, BarData.Close] - Input[i, BarData.Close]);
            }
            //return VOLATILITY;

            double _ER = DIRECTION / VOLATILITY; //{EFFICIENCY RATIO是AMA系统中最重要的指标，比值越大，趋势越明显}
            if (VOLATILITY == 0)
            {
                _ER = 1;
            }
            ER.Add(dt, _ER);
            //return ER;

            double FSC = 2.0 / (1.0 + p); // {快速平滑常数}
            double SSC = 2.0 / (1.0 + q); // {慢速平滑常数}
            double SC = _ER * (FSC - SSC) + SSC; //{等价于SC=ER*FSC+(1-ER)*SSC,指数平滑序列}
            double SCSQ = SC * SC;

            double db = Input[index, BarData.Close];
            if (this.Count < 1)
            {
            }
            else
            {
                db = SCSQ * db + (1 - SCSQ) * this.Last; //{DMA指标，AMAt=SCSQt-1*CLOSEt-1+(1-SCSQt-1)*AMAt-1}
            }

            return db;
        }
    }
}
