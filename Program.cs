using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HakutAI
{
    class Program
    {
        private const string subscriptionKey = 
            "Subsciption Key";
        private const string baseUri = 
            "https://northeurope.api.cognitive.microsoft.com/";


        private static readonly FaceClient faceClient = 
            new FaceClient(
            new ApiKeyServiceClientCredentials(subscriptionKey));


        
        public static async void DeleteGroup(string GroupId)
        {
            try
            {
                await faceClient.PersonGroup.DeleteAsync(GroupId);
                Console.WriteLine("Succeeded!");

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }
        public static async void CreatePersonGroup(string GroupId,
            string GroupName)
        {
            try
            {
                await faceClient.PersonGroup.CreateAsync(GroupId,
                    GroupName);
                Console.WriteLine("Succeeded!");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
            }
        }
        private static async void AddPersontoGroup(string GroupId,
            string PersonName, string imgPath)
        {
            try
            {
                await faceClient.PersonGroup.GetAsync(GroupId);
                Person person = await faceClient.PersonGroupPerson.
                    CreateAsync(GroupId, PersonName);
                foreach (var image in Directory.
                    GetFiles(imgPath, "*.jpg"))
                {
                    using (Stream stream = File.OpenRead(image))
                    {
                        await faceClient.PersonGroupPerson.
                            AddFaceFromStreamAsync(GroupId,
                            person.PersonId, stream);
                    }
                    Console.WriteLine("Succeeded...");
                }
                Console.WriteLine("All successed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR " + ex.Message);
            }
        }
        private static async void TrainAI(string GroupId)
        {
            await faceClient.PersonGroup.GetAsync(GroupId);
            await faceClient.PersonGroup.TrainAsync(GroupId);
            TrainingStatus training = null;
            while (true)
            {
                training = await faceClient.PersonGroup.
                    GetTrainingStatusAsync(GroupId);

                if (training.Status != TrainingStatusType.Running)
                {
                    Console.WriteLine("Status: " + training.Status);
                    break;
                }
                Console.WriteLine("Waiting for training...");
                await Task.Delay(1000);

            }
            Console.WriteLine("TRAINING COMPLETED SUCCESSFULLY!");


        }
        private static async void IdentifyFace(string GroupId, 
            string imgPath)
        {
            using (Stream stream = File.OpenRead(imgPath))
            {
                var faces = await faceClient.Face.
                    DetectWithStreamAsync(stream);
                var faceIds = faces.Select(face => face.
                FaceId.Value).ToArray();

                try
                {
                    await faceClient.PersonGroup.GetAsync
                        (GroupId);
                    var results = await faceClient.Face.
                        IdentifyAsync(faceIds,GroupId);
                    foreach (var identifyResult in results)
                    {
                        Console.WriteLine($"ID of the face is: " +
                            $"{identifyResult.FaceId}");
                        var personId = identifyResult.
                            Candidates[0].PersonId;
                        var person = await faceClient.
                            PersonGroupPerson.
                            GetAsync(GroupId, personId);
                        Console.WriteLine($"Identified as " +
                            $"{person.Name}");

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errror??? " + ex.Message);
                }
            }
        }
        static void Main(string[] args)
        {
            faceClient.Endpoint = baseUri;

            while(true)
            {
                Console.WriteLine("Pick one of the option:" +
                    "\n1.Delete Person Group" +
                    "\n2.Create Person Group" +
                    "\n3.Add Faces to Person Group" +
                    "\n4.Train the Person Group" +
                    "\n5.Identify a Face" +
                    "\nType 'Exit' for exit");

                String option = Console.ReadLine();
                String groupId, imgpath, personName,groupName;
                if(option == "exit")
                {
                    break;
                }

                switch(option)
                {
                    case "1":
                        Console.Write("Enter Group Id:\n");
                        groupId = Console.ReadLine();
                        DeleteGroup(groupId);
                        break;
                    case "2":
                        Console.WriteLine("Enter Group Id:\n");
                        groupId = Console.ReadLine();
                        Console.WriteLine("Enter Group Name:\n");
                        groupName = Console.ReadLine();
                        CreatePersonGroup(groupId, groupName);
                        break;
                    case "3":
                        Console.WriteLine("Enter Group Id:\n");
                        groupId = Console.ReadLine();
                        Console.WriteLine("Enter Person Name:\n");
                        personName = Console.ReadLine();
                        Console.WriteLine("Enter Data-set Path:\n");
                        imgpath = Console.ReadLine();
                        AddPersontoGroup(groupId, personName, @imgpath);
                        break;
                    case "4":
                        Console.WriteLine("Enter Group Id to Train:\n");
                        groupId = Console.ReadLine();
                        TrainAI(groupId);
                        break;
                    case "5":
                        Console.WriteLine("Enter Group Id:\n");
                        groupId = Console.ReadLine();
                        Console.WriteLine("Enter Image Path:\n");
                        imgpath = Console.ReadLine();
                        IdentifyFace(groupId, @imgpath);
                        break;



                }

                Console.ReadKey();
            }

            
        }
    }
}
