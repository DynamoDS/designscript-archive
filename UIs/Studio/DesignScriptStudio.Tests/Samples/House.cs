using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScriptStudio.Tests.Samples
{
    public class House
    {
        double width = 0;
        double height = 0;
        double floors = 0;

        #region Public Construction Methods

        public static House ByPrice(int price)
        {
            House house = new House();
            house.width = 3 * price;
            house.height = 4 * price;
            house.floors = 1;
            return house;
        }

        public static House ByWidthHeight(double width, double height)
        {
            if (width <= 0 || (height <= 0))
                return null;

            House house = new House();
            house.width = width;
            house.height = height;
            house.floors = 1;
            return house;
        }

        public static House ByWidthHeightFloors(double width, double height, int floors)
        {
            if (width <= 0 || (height <= 0))
                return null;
            if (floors <= 0 || (floors > 10))
                return null;

            House house = new House();
            house.width = width;
            house.height = height;
            house.floors = floors;
            return house;
        }

        #endregion

        #region Public Class Properties

        public double Width { get { return width; } }
        public double Height { get { return height; } }

        #endregion

        #region Private Class Helper Methods

        private House()
        {
        }

        #endregion
    }
}
