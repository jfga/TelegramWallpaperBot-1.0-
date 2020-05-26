using System;

using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TelegramWallpaperBot_1._0_
{
    class Program
    {
        static TelegramBotClient Bot;
        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("The key") { Timeout = TimeSpan.FromSeconds(10) };
            var me = Bot.GetMeAsync().Result;

            Bot.OnCallbackQuery += Bot_OnCallbackQueryReseived;


            Bot.OnMessage += BotOnMassegeReseived;
            Console.WriteLine(me.FirstName);
            Bot.StartReceiving();
            Console.ReadLine();
            //Bot.StopReceiving();
            

        }

        
        private static async void Bot_OnCallbackQueryReseived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string[] userRequest = e.CallbackQuery.Data.Split('※');

            switch (userRequest[1])
            {
                case "1":                                                                        //search by color
                    {
                        FindUser findUser = new FindUser();
                        findUser.color = userRequest[0];

                        var json = JsonConvert.SerializeObject(findUser);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using var client = new HttpClient();
                        var content = await client.PostAsync("https://wallpaperapi.azurewebsites.net/api/Wallpaper/color", data);
                        string result = content.Content.ReadAsStringAsync().Result;

                        WallhavenResponse wallhaven = JsonConvert.DeserializeObject<WallhavenResponse>(result);

                        if (result == "BAD")
                        {
                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Very bad");
                            break;
                        }
                        if  (wallhaven.results.Count  ==  null  || wallhaven.results.Count == 0)
                        {
                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Very bad");
                            break;
                        }

                        List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();

                        int a = 0;

                        foreach (var photo in wallhaven.results)    //dynamic buttons
                        {
                            List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                                ts.Add(InlineKeyboardButton.WithUrl(photo.id, photo.urls.full));
                                ts.Add(InlineKeyboardButton.WithCallbackData("Add", photo.id+ "※3"));
                                inlineKeyboardList.Add(ts);
                        }

                        var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                        await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "________________________________________", replyMarkup: inline);
                        break;

                    }
                case "2":                                              // search by category
                    {
                        FindUser findUser = new FindUser();
                        findUser.category = userRequest[0];

                        var json = JsonConvert.SerializeObject(findUser);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using var client = new HttpClient();
                        var content = await client.PostAsync("https://wallpaperapi.azurewebsites.net/api/Wallpaper/category", data); 
                        string result = content.Content.ReadAsStringAsync().Result;

                        WallhavenResponse wallhaven = JsonConvert.DeserializeObject<WallhavenResponse>(result);

                        if (result == "BAD")
                        {
                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Very bad");
                            break;
                        }
                        if (wallhaven.results.Count == null || wallhaven.results.Count == 0)
                        {
                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "Very bad");
                            break;
                        }

                        List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();

                        int a = 0;

                        foreach (var photo in wallhaven.results)
                        {
                            List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                            ts.Add(InlineKeyboardButton.WithUrl(photo.id, photo.urls.full));
                            ts.Add(InlineKeyboardButton.WithCallbackData("Add", photo.id + "※3"));
                            inlineKeyboardList.Add(ts);
                        }
                        
                        var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                        await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "________________________________________", replyMarkup: inline);
                        break;

                    }
                case "3":                                                //add to favourite
                    {
                        try
                        {
                            Console.WriteLine("Using DB");

                            FindUser findUser = new FindUser();
                            findUser.id = e.CallbackQuery.From.Id;
                            findUser.photoId = userRequest[0];

                            var json = JsonConvert.SerializeObject(findUser);
                            var data = new StringContent(json, Encoding.UTF8, "application/json");

                            using var client = new HttpClient();
                            var content = await client.PostAsync("https://wallpaperapi.azurewebsites.net/api/Wallpaper/addfavourite", data);
                            string result = content.Content.ReadAsStringAsync().Result;

                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, result);
                            break;
                        }
                        catch
                        {
                            Console.WriteLine("Exeption");
                            break;
                        }
                    }
                case "4":                                           //delete from favourite
                    {
                        try
                        {
                            Console.WriteLine("Using DB");

                            FindUser findUser = new FindUser();
                            findUser.id = e.CallbackQuery.From.Id;
                            findUser.photoId = userRequest[0];

                            var json = JsonConvert.SerializeObject(findUser);
                            var data = new StringContent(json, Encoding.UTF8, "application/json");

                            using var client = new HttpClient();
                            var content = await client.PutAsync("https://wallpaperapi.azurewebsites.net/api/Wallpaper/delfavourite", data);
                            string result = content.Content.ReadAsStringAsync().Result;

                            await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, result);
                            break;
                        }
                        catch
                        {
                            Console.WriteLine("Exeption");
                            break;
                        }
                    }
                default:
                    {
                        Console.WriteLine("Doupe");
                        break;
                    }
            }
            string name = $"{e.CallbackQuery.From.FirstName}  {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"{name} tap on {userRequest}");
        }

        public static async void BotOnMassegeReseived( object sendere, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text)
                return;



            string name = $"{message.From.FirstName} {message.From.LastName} ";
            Console.WriteLine($"{name} send {message.Text}");

            switch (message.Text)
            {
                case "/start":
                    string text =
@"
This bot has several commands, check it out before you start.
List of commands:
/searchbygenre - this command will give you several genres for image search. 
Choose ganre and click on the link issued to open the image. Click 'Add' to add to your favorite list.
/searchbycolor - this command provides the ability to search pictures by a specific color. 
If you have favourite color, this comand special 4u.
/random - Random is the most interesting comand that generates a random image.
It's useful if your imagination runs out. Try your luck)
/favourite - the comand gives a list of favorite photos that can be saved by clicking the Add button while you using bot.
You can make changes to the list by deleting items from it. Just click on the 'Delete' button.

Еnjoy using!
";
                  await  Bot.SendPhotoAsync(message.From.Id, "https://images.unsplash.com/photo-1477959858617-67f85cf4f1df?ixlib=rb-1.2.1&q=85&fm=jpg&crop=entropy&cs=srgb&ixid=eyJhcHBfaWQiOjEzMTIxNX0");
                  await  Bot.SendTextMessageAsync(message.From.Id, text);
                                             
                    
                    break; 
                case "/searchbycolor":
                    {
                        var keyboard = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Black and White","Black and White※1"),
                            InlineKeyboardButton.WithCallbackData("Pink", "Pink※1" ),
                            InlineKeyboardButton.WithCallbackData("Yellow", "Yellow※1")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Blue", "Blue※1"),
                            InlineKeyboardButton.WithCallbackData("Red", "Red※1"),
                            InlineKeyboardButton.WithCallbackData("Green", "Green※1")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Purpure", "Purpure※1"),
                            InlineKeyboardButton.WithCallbackData("Orange", "Orange※1"),
                            InlineKeyboardButton.WithCallbackData("Brown", "Brown※1")
                        }
                        });

                        await Bot.SendPhotoAsync(message.From.Id,
                            "https://images.unsplash.com/photo-1433888104365-77d8043c9615?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=1053&q=80");

                        await Bot.SendTextMessageAsync(message.From.Id, "Select color for serch", replyMarkup: keyboard);
                        break;
                    }
                case "/searchbygenre": 
                    {

                        var genrekeyboard = new InlineKeyboardMarkup(new[]
                            {
                            new[]{
                            InlineKeyboardButton.WithCallbackData("anime","anime※2"),
                            InlineKeyboardButton.WithCallbackData("general","general※2"),
                            InlineKeyboardButton.WithCallbackData("NSFW","NSFW※2"),
                            InlineKeyboardButton.WithCallbackData("Abstract","Abstract※2")
                            },
                            new[]
                            {
                             InlineKeyboardButton.WithCallbackData("Astrophotography","Astrophotography※2"),
                             InlineKeyboardButton.WithCallbackData("Architecture","Architecture※2"),
                             InlineKeyboardButton.WithCallbackData("City","City※2"),
                             InlineKeyboardButton.WithCallbackData("Family","Family※2")
                            },
                            new[]
                            {
                             InlineKeyboardButton.WithCallbackData("Landscape","Landscape※2"),
                             InlineKeyboardButton.WithCallbackData("Panorama","Panorama※2"),
                             InlineKeyboardButton.WithCallbackData("Drone","Drone※2"),
                             InlineKeyboardButton.WithCallbackData("War","War※2")
                            }
                            });

                        await Bot.SendPhotoAsync(message.From.Id,
                            "https://images.unsplash.com/photo-1504805572947-34fad45aed93?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=1050&q=80");
                             
                            await Bot.SendTextMessageAsync(message.From.Id, "Choose ganre", replyMarkup: genrekeyboard );
                            break;
                        }
                case "/random": 
                    {
                        using var client = new HttpClient();
                        var content = await client.GetAsync("https://api.unsplash.com/photos/random/?client_id=JGYSsjXO8ANdFCsiBrNZVmi3yXfOcSM5VD0jU8EpeY8");
                        string result = content.Content.ReadAsStringAsync().Result;

                        RandomResponse randomResponse = JsonConvert.DeserializeObject<RandomResponse>(result);
                        
                        var keyboardrandom = new InlineKeyboardMarkup(new[]
                        {
                            new[]{ InlineKeyboardButton.WithUrl("PHOTO",randomResponse.urls.full)},
                            new[]{ InlineKeyboardButton.WithCallbackData("Add to favourite",randomResponse.id+ "※3") } 
                        });
                        await Bot.SendPhotoAsync(e.Message.Chat.Id, 
                            "https://images.unsplash.com/photo-1458419948946-19fb2cc296af?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=1050&q=80");

                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Random photo", replyMarkup: keyboardrandom);
                                             break;
                    }

                case "/favourite":
                    {
                        FindUser findUser = new FindUser();
                        findUser.id = (int)e.Message.Chat.Id;

                        var json = JsonConvert.SerializeObject(findUser);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        using var client = new HttpClient();
                        var content = await client.PostAsync("https://wallpaperapi.azurewebsites.net/api/Wallpaper/getfavourite",data);
                        string result = content.Content.ReadAsStringAsync().Result;

                        if (result == "BAD")
                        {
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Very bad");
                            break;
                        }

                        List<List<InlineKeyboardButton>> inlineKeyboardList = new List<List<InlineKeyboardButton>>();

                        Users users = new Users();
                        List<PhotoInfo> example = new List<PhotoInfo>();
                        example = JsonConvert.DeserializeObject<List<PhotoInfo>>(result);
                        users.Images  =  example;

                        int a = 0;

                        
                        if (users.Images.Count==0|| users.Images.Count == null)
                        {
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "No image");
                            break;
                        }


                        foreach (var photo in users.Images)                                           //dynamic buttons
                        {
                            List<InlineKeyboardButton> ts = new List<InlineKeyboardButton>();
                            ts.Add(InlineKeyboardButton.WithUrl(photo.Id, photo.Link));
                            ts.Add(InlineKeyboardButton.WithCallbackData("Delete", photo.Id + "※4"));
                            inlineKeyboardList.Add(ts);
                        }
                        var inline = new InlineKeyboardMarkup(inlineKeyboardList);
                   

                        await Bot.SendPhotoAsync(e.Message.From.Id,
                            "https://images.unsplash.com/photo-1580571313472-5bd3de1d1ab8?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=500&q=60");
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "________________________________________", replyMarkup: inline);
                        

                        break;

                    }
                default:
                    break;

            }
        }
    }
}
