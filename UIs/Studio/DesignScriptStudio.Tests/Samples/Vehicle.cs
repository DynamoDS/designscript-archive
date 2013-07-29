using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScriptStudio.Tests.Samples
{
    public class Vehicle
    {
        int price = 0, color = 0;
        string model = string.Empty;
        string brand = string.Empty;

        #region Public Construction Methods

        public static Vehicle ByPrice(int price)
        {
            Vehicle vehicle = new Vehicle();
            vehicle.price = price;
            return vehicle;
        }

        public static Vehicle ByPriceColor(int price, int color)
        {
            Vehicle vehicle = new Vehicle();
            vehicle.price = price;
            vehicle.color = color;
            return vehicle;
        }

        public static Vehicle ByBrand(string brand)
        {
            Vehicle vehicle = new Vehicle();
            vehicle.brand = brand;
            return vehicle;
        }

        public static Vehicle ByBrandModel(string brand, string model)
        {
            Vehicle vehicle = new Vehicle();
            vehicle.brand = brand;
            vehicle.model = model;
            return vehicle;
        }

        public static Vehicle ByBrandModelColor(string brand, string model, int color)
        {
            Vehicle vehicle = new Vehicle();
            vehicle.brand = brand;
            vehicle.model = model;
            vehicle.color = color;
            return vehicle;
        }

        #endregion

        #region Public Class Properties

        public int Price { get { return price; } }
        public int Color { get { return color; } }
        public string Brand { get { return brand; } }
        public string Model { get { return model; } }

        #endregion

        #region Private Class Helper Methods

        private Vehicle()
        {
        }

        #endregion

    }
}
