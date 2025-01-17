﻿using Casualty_Radar.Models;

namespace Casualty_Radar.Utils {
    /// <summary>
    /// A static collection containing alert information.
    /// This collection is used to fetch readable information based on unreadable alert details.
    /// </summary>
    static class AlertUtil {
        public static string[,] P2000 = {
            {"A1", "1", "Ambulance", "Hoogste Prioriteit"},
            {"A2", "1", "Ambulance", "Normale Prioriteit"},
            {"B", "1", "Ambulance", "Besteld vervoer"},
            {"P 1", "2", "Brandweer", "Spoed rit"},
            {"P 2", "2", "Brandweer", "Gepaste spoed rit"},
            {"P 3", "2", "Brandweer", "Geen spoed"},
            {"P 4", "2", "Brandweer", "Testmelding"},
            {"P 5", "2", "Brandweer", "Testmelding"},
            {"PRIO 1", "2", "Brandweer", "Spoed rit"},
            {"PRIO 2", "2", "Brandweer", "Gepaste spoed rit"},
            {"PRIO 3", "2", "Brandweer", "Geen spoed"},
            {"PRIO 4", "2", "Brandweer", "Testmelding"},
            {"PRIO 5", "2", "Brandweer", "Testmelding"},
            {"PR 1", "2", "Brandweer", "Spoed rit"},
            {"PR 2", "2", "Brandweer", "Gepaste spoed rit"},
            {"PR 3", "2", "Brandweer", "Geen spoed"},
            {"PR 4", "2", "Brandweer", "Testmelding"},
            {"PR 5", "2", "Brandweer", "Testmelding"},
            {"BR", "2", "Brandweer", "Brand"},
            {"HV", "2", "Brandweer", "Hulpverlening"},
            {"VKO", "2", "Brandweer", "Verkeersongeval"},
            {"WO", "2", "Brandweer", "Waterongeval"},
            {"DV", "2", "Brandweer", "Dienstverlening"}
        };

        /// <summary>
        /// Appends extra attributes and properties to a created alert.
        /// </summary>
        /// <param name="alert">The original alert instance</param>
        /// <param name="alertItemString">The original, slightly modified, data string called from the Feed class</param>
        /// <returns>A detailed alert containing details and priority levels</returns>
        public static Alert SetAlertAttributes(Alert alert, string alertItemString) {
            for (int i = 0; i < P2000.GetLength(0); i++) {
                if (alertItemString.StartsWith(P2000[i, 0])) {
                    alert.Code = P2000[i, 0];
                    alert.Type = int.Parse(P2000[i, 1]);
                    alert.TypeString = P2000[i, 2];
                    alert.Info = P2000[i, 3];
                    return alert;
                }
            }
            return null;
        }
    }
}