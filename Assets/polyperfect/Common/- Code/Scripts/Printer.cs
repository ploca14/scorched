using UnityEngine;

namespace Polyperfect.Common
{
    public class Printer:PolyMono
    {
        public override string __Usage => "Allows printing messages to the Console using UnityEvents.";

        public void Print(string message) => Debug.Log(message);
        public void PrintWarning(string message) => Debug.LogWarning(message);
        public void PrintError(string message) => Debug.LogError(message);
    }
}