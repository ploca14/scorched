using EasyBuildSystem.Features.Scripts.Helpers;
using EasyBuildSystem.Runtimes.Internal.Storage.Structs;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Storage.Data
{
    [System.Serializable]
    public class PieceData
    {
        #region Fields

        public List<SerializedPiece> Pieces = new List<SerializedPiece>();

        [System.Serializable]
        public class SerializedPiece
        {
            [SerializeField] public string Id;
            [SerializeField] public string Name;
            [SerializeField] public int SkinIndex;
            [SerializeField] public string Parent;
            [SerializeField] public SerializeVector3 Position;
            [SerializeField] public SerializeVector3 Rotation;
            [SerializeField] public SerializeVector3 Scale;
            [SerializeField] public List<string> Properties = new List<string>();
        }

        #endregion Fields

        #region Methods

        /// <summary>
        /// This method return the prefabs encode to custom string.
        /// </summary>
        public string ToJson()
        {
            return JsonHelper.ToJson(Pieces.ToArray(), true);
        }

        /// <summary>
        /// This method return the prefabs decode from custom string.
        /// </summary>
        public SerializedPiece[] DecodeToStr(string data)
        {
            return JsonHelper.FromJson<SerializedPiece>(data);
        }

        /// <summary>
        /// This method return a Vector3 from a string Vector3.
        /// </summary>
        public static Vector3 ToVector3(string strVector)
        {
            if (strVector.StartsWith("(") && strVector.EndsWith(")"))
            {
                strVector = strVector.Substring(1, strVector.Length - 2);
            }

            string[] Data = strVector.Split(',');

            Vector3 result = new Vector3(
                float.Parse(Data[0], CultureInfo.InvariantCulture),
                float.Parse(Data[1], CultureInfo.InvariantCulture),
                float.Parse(Data[2], CultureInfo.InvariantCulture));

            return result;
        }

        /// <summary>
        /// This method return a serialized Vector3 in a Vector3.
        /// </summary>
        public static Vector3 ParseToVector3(SerializeVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// This method return a Vector3 in a serialized Vector3.
        /// </summary>
        public static SerializeVector3 ParseToSerializedVector3(Vector3 vector)
        {
            SerializeVector3 SerializedVector3 = new SerializeVector3
            {
                X = vector.x,
                Y = vector.y,
                Z = vector.z
            };

            return SerializedVector3;
        }

        #endregion Methods
    }
}