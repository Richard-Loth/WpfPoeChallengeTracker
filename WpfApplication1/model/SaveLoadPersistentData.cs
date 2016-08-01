using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace WpfPoeChallengeTracker.model
{
    class SaveLoadPersistentData
    {


        private static string makeFilename(string leaguename)
        {
            return "challengeProgressData" + "." + leaguename.ToLower() + ".pct";
        }

        public static void deleteSavedProgress(string leaguename)
        {
           //TODO
        }

        public static void saveProgressAndOrderAsync(List<ChallengeProgress> progress, List<int> order,  string leaguename)
        {
            var data = new ProgressOrderContainer(progress, order);
            saveData(data, leaguename);
        }

        private static void saveData(ProgressOrderContainer data, string leaguename)
        {
            try
            {
                //uwp implementation:
                //StorageFile saveFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(makeFilename(leaguename), CreationCollisionOption.ReplaceExisting);
                //DataContractSerializer serializer = new DataContractSerializer(data.GetType());
                //var targetStream = await saveFile.OpenStreamForWriteAsync();
                //serializer.WriteObject(targetStream, data);
                //targetStream.Dispose();
                var savePath = getSavePath();
                XmlSerializer serializer = new XmlSerializer(data.GetType());
                var filepath = Path.Combine(savePath, makeFilename(leaguename));
                Directory.CreateDirectory(savePath);
                var writer = new StreamWriter(filepath);
                serializer.Serialize(writer, data);
                writer.Dispose();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error during saving. Reason: " + e.Message);
                throw new Exception("Error during saving", e);
            }
        }

        private static string getSavePath()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory + "\\save";
        }

        public static ProgressOrderContainer loadData(string leaguename)
        {
            try
            {
                //StorageFolder storageFolder =
                //    ApplicationData.Current.LocalFolder;
                //if (await storageFolder.TryGetItemAsync(makeFilename(leaguename)) == null)
                //{
                //    //no saved data
                //    return null;
                //}
                //StorageFile saveFile =
                //    await storageFolder.GetFileAsync(makeFilename(leaguename));
                //Stream stream = await saveFile.OpenStreamForReadAsync();
                //DataContractSerializer serializer = new DataContractSerializer(typeof(ProgressOrderContainer));
                //ProgressOrderContainer container = (ProgressOrderContainer)serializer.ReadObject(stream);
                //stream.Dispose();
                //return container;

                string savepath = getSavePath();
                var filepath = Path.Combine(savepath, makeFilename(leaguename));
                if (!File.Exists(filepath))
                {
                    //no saved data
                    return null;
                }

                var deserializer = new XmlSerializer(typeof(ProgressOrderContainer));
                var reader = new StreamReader(filepath);
                var savedObject = deserializer.Deserialize(reader);
                reader.Dispose();
                return (ProgressOrderContainer) savedObject;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error during loading. Reason: " + e.Message);
                return null;
            }
        }
    }

    public class ProgressOrderContainer
    {
        public List<ChallengeProgress> progress;
        public List<int> order;

        public ProgressOrderContainer()
        {

        }

        public ProgressOrderContainer(List<ChallengeProgress> progress, List<int> order)
        {
            this.progress = progress;
            this.order = order;
        }
    }
}
