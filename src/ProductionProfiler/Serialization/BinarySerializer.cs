using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProductionProfiler.Core.Serialization
{
    /// <summary>
    /// This is a helper class used to binary serialize/deserialize objects
    /// </summary>
    /// <typeparam name="T">Object type to serialize</typeparam>
    public static class BinarySerializer<T>
    {
        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="deserializedObject">The deserialized object.</param>
        /// <returns></returns>
        public static byte[] Serialize(T deserializedObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //Serialize the object to the stream
                new BinaryFormatter().Serialize(stream, deserializedObject);

                //instantiate the byte[] to hold the serialized data
                byte[] bytes = new byte[stream.Length];

                //read the stream into the byte[]
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytes, 0, (int)stream.Length);

                //return the bytes
                return bytes;
            }
        }

        /// <summary>
        /// This will deserialize an object for the type <see cref="T"/>
        /// This method does not close/dispose of the stream, calling code must do this
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// An instance of the type <see cref="T"/>
        /// </returns>
        public static T Deserialize(byte[] serializedObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //read the serialized object into the stream
                stream.Write(serializedObject, 0, serializedObject.Length);
                stream.Seek(0, SeekOrigin.Begin);

                //instantiate the BinaryFormatter and deserialize the object
                return (T)new BinaryFormatter().Deserialize(stream);
            }
        }

        /// <summary>
        /// Performs a deep copy of the object parameter using the BinaryFormatter
        /// </summary>
        /// <param name="deserializedObject">The deserialized object.</param>
        /// <returns></returns>
        public static T Clone(T deserializedObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //instantiate the BinaryFormatter
                IFormatter formatter = new BinaryFormatter();

                //Serialize the object to the memory stream
                formatter.Serialize(stream, deserializedObject);

                //reset the stream position
                stream.Position = 0;

                //de-serialize the stream back to the original type
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}