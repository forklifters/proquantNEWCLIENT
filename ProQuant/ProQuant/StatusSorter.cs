using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ProQuant
{
    class StatusSorter
    {
        public static Color JobNumberColor(Job job)
        {
            Color color;

            switch (job.status)
            {
                case "S20":
                    color = Color.Green;
                    break;
                case "S02":
                    color = Color.Red;
                    break;
                case "S99":
                    color = Color.Gray;
                    break;
                default:
                    color = Color.Black;
                    break;
            }
            return color;
        }

        public static Color StatusColor(Job job)
        {
            Color color;

            switch (job.status)
            {
                case "S20":
                    color = Color.Green;
                    break;
                case "S02":
                    color = Color.Red;
                    break;
                case "S03":
                    color = Color.Purple;
                    break;
                case "S99":
                    color = Color.DarkGray;
                    break;
                case "S06":
                    if (job.grossValue.ToString() == "" || job.grossValue.ToString() == null)
                    {
                        color = Color.DeepSkyBlue;
                    }
                    else
                    {
                        color = Color.Red;
                    }
                    break;
                default:
                    color = Color.Black;
                    break;
            }
            return color;
        }

        public static Color CellColor(Job job)
        {
            Color color;

            switch (job.status)
            {
                case "S02":
                    color = Color.LightPink;
                    break;
                case "S99":
                    color = Color.LightGray;
                    break;
                default:
                    color = Color.White;
                    break;
            }
            return color;
        }

        public static string StatusText(Job job)
        {
            string text;

            switch (job.status)
            {
                case "S20":
                    text = "Completed";
                    break;
                case "S00":
                    text = "Awaiting Appraisal";
                    break;
                case "S02":
                    text = "Awaiting Documentation";
                    break;
                case "S03":
                    text = "Adjudication Issue";
                    break;
                case "S99":
                    text = "Cancelled";
                    break;
                case "S06":
                    if (job.grossValue.ToString() == "" || job.grossValue.ToString() == null)
                    {
                        text = "Being Priced";
                    }
                    else
                    {
                        text = "Unpaid";
                    }
                    break;
                default:
                    text = job.status;
                    break;
            }
            return text;
        }

        //switch (subjobs[0].status)
        //            {
        //                case "S20":
        //                    _job.JobColor = Color.Green;
        //                    _job.StatusColor = Color.Green;
        //                    _job.Status = "Completed";
        //                    break;
        //                case "S00":
        //                    _job.Status = "Awaiting Appraisal";
        //                    break;
        //                case "S02":
        //                    _job.CellColor = Color.LightPink;
        //                    _job.JobColor = Color.Red;
        //                    _job.StatusColor = Color.Red;
        //                    _job.Status = "Awaiting Documentation";
        //                    break;
        //                case "S03":
        //                    _job.Status = "Adjudication Issue";
        //                    _job.StatusColor = Color.Purple;
        //                    break;
        //                case "S99":
        //                    _job.Status = "Cancelled";
        //                    _job.CellColor = Color.LightGray;
        //                    break;
        //                case "S06":
        //                    if (subjobs[0].grossValue.ToString() == "" || subjobs[0].grossValue.ToString() == null)
        //                    {
        //                        _job.Status = "Being Priced";
        //                        _job.StatusColor = Color.DeepSkyBlue;
        //                    }
        //                    else
        //                    {
        //                        _job.Status = "Unpaid";
        //                        _job.StatusColor = Color.Red;
        //                    }
        //                    break;


        //                default:
        //                    _job.Status = subjobs[0].status;
        //                    break;
        //            }

    }
}
