// See https://aka.ms/new-console-template for more information
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

if (args.Length < 1)
{
    Console.WriteLine("Usage: gmailOauthSend <to>");
    return;
}

var to = args[0];


var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.Development.json", optional: false)
    .Build();

var gmailSection = config.GetSection("Gmail");
var fromAddress = gmailSection["From"];
var clientId = gmailSection["ClientId"];
var clientSecret = gmailSection["ClientSecret"];
var scopes = new[] { "https://mail.google.com/" };
var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
    new ClientSecrets
    {
        ClientId = clientId,
        ClientSecret = clientSecret
    },
    scopes,
    "user",
    CancellationToken.None,
    new FileDataStore("GmailTokenStore")
);

var token = credential.Token.AccessToken;

var message = new MimeMessage();
message.From.Add(new MailboxAddress("Your Name", fromAddress));
message.To.Add(new MailboxAddress("", to));
message.Subject = "TestMail";
message.Body = new TextPart("plain")
{
    Text = "This is a test mail."
};

using (var client = new SmtpClient())
{
    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
    client.Authenticate(new SaslMechanismOAuth2(fromAddress, token));
    client.Send(message);
    client.Disconnect(true);
}
