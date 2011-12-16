﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVERouteFinder.Classes
{
    class EVEOrder
    {

        long orderID;
        int regionID;
        int systemID;
        int stationID;
        int typeID;
        int bid;
        double price;
        int minVolume;
        int volRemain;
        int volEnter;
        DateTime issued;
        TimeSpan duration;
        DateTime reported;

        public long OrderID { get { return this.orderID; } set { this.orderID = value;} }
        public int RegionID { get { return this.regionID;} set { this.regionID = value;} }
        public int SystemID { get { return this.systemID;} set { this.systemID = value;} }
        public int StationID { get { return this.stationID;} set { this.stationID = value;} }
        public int TypeID { get { return this.typeID; } set { this.typeID = value; } }
        public int Bid { get { return this.bid; } set { this.bid = value; } } //bid = 0 means it's a sell order, bid=1 means it's a buying order
        public double Price { get { return this.price; } set { this.price = value; } }
        public int MinVolume { get { return this.minVolume; } set { this.minVolume = value; } }
        public int VolRemain { get { return this.volRemain; } set { this.volRemain = value; } }
        public int VolEnter { get { return this.volEnter; } set { this.volEnter = value; } }
        public DateTime Issued { get { return this.issued; } set { this.issued = value; } }
        public TimeSpan Duration { get { return this.duration; } set { this.duration = value; } }
        public DateTime Reported { get { return this.reported; } set { this.reported = value; } }

        public EVEOrder(string[] order)
        {
            bool success = false;
            if (order[6].Contains('.')) //then it's CSV
            {
                if (long.TryParse(order[0], out this.orderID) && 
                    int.TryParse(order[1], out this.regionID) && 
                    int.TryParse(order[2], out this.systemID) &&
                    int.TryParse(order[3], out this.stationID) &&
                    int.TryParse(order[4], out this.typeID) &&
                    int.TryParse(order[5], out this.bid) &&
                    double.TryParse(order[6], System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.CultureInfo("en-US"), out this.price) &&
                    int.TryParse(order[7], out this.minVolume) &&
                    int.TryParse(order[8], out this.volRemain) &&
                    int.TryParse(order[9], out this.volEnter) &&
                    DateTime.TryParse(order[10], out this.issued) &&
                    DateTime.TryParse(order[14], out this.reported))
                    {
                        string s = order[11].Replace(" days, ", ":");
                        if (s == order[11])
                        {
                            s = order[11].Replace(" day, ", ":");
                        }
                        success = TimeSpan.TryParse(s, out this.duration);
                    }
            }
            else //then it's DB dump
            {
                if (long.TryParse(order[6], out this.orderID) &&
                    int.TryParse(order[0], out this.regionID) &&
                    int.TryParse(order[1], out this.systemID) &&
                    int.TryParse(order[2], out this.stationID) &&
                    int.TryParse(order[3], out this.typeID) &&
                    int.TryParse(order[4], out this.bid) &&
                    double.TryParse(order[5], System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.CultureInfo("en-US"), out this.price) &&
                    int.TryParse(order[7], out this.minVolume) &&
                    int.TryParse(order[8], out this.volRemain) &&
                    int.TryParse(order[9], out this.volEnter) &&
                    DateTime.TryParse(order[10], out this.issued) &&
                    DateTime.TryParse(order[14], out this.reported))
                {
                    string s = order[11].Replace(" days", ":00:00:00");
                    
                    if (s == order[11])
                    {
                        s = order[11].Replace(" day", ":00:00:00");
                    }
                    success = TimeSpan.TryParse(s, out this.duration);
                }
            }
            if (!success)
            {
                throw new ArgumentException("The provided string does not match the market dump data order nor the CSV data order!");
            }
        }

        public bool InsertToDB()
        {
            int rows=-1;
            EVEDBoperations orderOperations = new EVEDBoperations();
            orderOperations.startEVEDBConnection(true);
            orderOperations.openEVEDBConnection();
            orderOperations.setEVEDBQuery(orderOperations.premadeQuery_insertToEveMarketData(this));
            rows = orderOperations.eveDBExecuteNonQuery();
            orderOperations.closeEVEDBConnection();
            return rows > 0;
        }
        //DB dump data order
        //regionid,
        //systemid,
        //stationid,
        //typeid, 
        //bid, 
        //price, 
        //orderid, 
        //minvolume, 
        //volremain, 
        //volenter, 
        //issued, 
        //duration, 
        //range, 
        //reportedby, 
        //reportedtime

        //CSV data order
        //"orderid",
        //"regionid",
        //"systemid",
        //"stationid",
        //"typeid",
        //"bid",
        //"price",
        //"minvolume",
        //"volremain",
        //"volenter",
        //"issued",
        //"duration",
        //"range",
        //"reportedby",
        //"reportedtime"

    }
}