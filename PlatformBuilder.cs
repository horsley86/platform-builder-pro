using System;
using UnityEngine;
using System.Collections.Generic;

namespace PlatformBuilderPro
{
    /*
     * this class is used to control the different strategies 
     * (note: a strategy is like a plugin and adds additional functionality/operations)
     */
    [Serializable]
    public class PlatformBuilder
    {
        [SerializeField]
        private PlatformBuilderStrategy _strategy;

        //called by the platform class, so it can keep track of the active strategy
        public void SetStrategy(PlatformBuilderStrategy strategy, Platform platform)
        {
            _strategy = strategy;
            _strategy.SetParent(platform);
        }

        //called by the core class, so that we can pre-process any operation (strategy) before the mesh is built
        public PlatformUpdateInfo Update(PlatformPoint[][] points)
        {
            //create a struct to hold points and whether or not the core should update
            //with the shouldUpdate flag, a strategy can control when an update in the core should occur.
            var updateInfo = new PlatformUpdateInfo { points = points, shouldUpdate = true };

            //if there is no active strategy, then return the struct as is
            if (_strategy == null) return updateInfo;

            //otherwise pass the struct off to the active strategy and return the results
            return _strategy.UpdatePoints(updateInfo);
        }

        /*
         * construct a list of available strategies in the 'Strategies' folder
         * (note: in order for a strategy to be recognized, you must make sure that the
         * strategy class name is the folder name + 'Strategy')
         * Example:
         * -Strategies (Directory)
         *  |--Bezier (Directory)
         *     |--Bezier + "Strategy".cs (strategy class file name)
         */
        public static PlatformBuilderStrategy[] GetStrategies()
        {
            var platformBuilderStrategyList = new List<PlatformBuilderStrategy>();
            var strategyStrings = System.IO.Directory.GetDirectories("Assets/platform-builder-pro/strategies");
            foreach (var strategyString in strategyStrings)
            {
                var strategyStringArray = strategyString.Split('\\');
                var strategyName = strategyStringArray[strategyStringArray.Length - 1] + "Strategy";
                platformBuilderStrategyList.Add((PlatformBuilderStrategy)ScriptableObject.CreateInstance(strategyName));
            }
            return platformBuilderStrategyList.ToArray();
        }
    }
}