namespace SnaffCore.Config
{
    public partial class Options
    {
        [Nett.TomlIgnore]
        public string[] GrepStrings { get; set; } =
        {
            // these are strings that make a file interesting if found within.
            "net user ",
            "net localgroup ",
            "psexec ",
            "runas ",
            " -Credential ",
            " -AsSecureString",
            //"jdbc",
            //"odbc",
            "password",
            "PRIVATE KEY----",
            " connectionString=\"",
            "sqlConnectionString=\"",
            "validationKey=",
            "decryptionKey=",
            //"credential",
            //"root:",
            //"admin:",
            //"login",
            "cpassword"
        };

        [Nett.TomlIgnore]
        public string[] ExtensionsToGrep { get; set; } =
        {
            // these are file extensions that tell us the file is worth grepping.
            ".ps1",
            ".bat",
            ".wsf",
            ".vbs",
            ".pl",
            ".txt",
            ".cs",
            ".ascx",
            ".java",
            ".config",
            ".ini",
            ".inf",
            ".cnf",
            ".conf",
            ".py",
            ".php",
            ".aspx",
            ".ashx",
            ".asmx",
            ".asp",
            ".jsp",
            ".yaml",
            ".xml",
            ".json",
            ".psd1",
            ".psm1",
            ".sh",
            ".cshtml",
            ".sql",
            ".pem",
            ".log"
        };

        // regexes

/*public class Regexes
{

    Regex privkey = new Regex(@"-----BEGIN [EC|RSA|DSA|OPENSSH] PRIVATE KEY----", 
        RegexOptions.Compiled);

    Regex awsaccesskey = new Regex(@"(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}",
        RegexOptions.Compiled);

    Regex awsaccountid = new Regex(@"((\\\"|'|`)?((?i)aws)?_?((?i)account)_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[0-9]{4}-?[0-9]{4}-?[0-9]{4}(\\\"|'|`)?)",
        RegexOptions.Compiled);

    Regex awssecretkey = new Regex(@"((\\\"|'|`)?((?i)aws)?_?((?i)secret)_?((?i)access)?_?((?i)key)?_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[A-Za-z0-9/+=]{40}(\\\"|'|`)?)",
        RegexOptions.Compiled);
    /*

- part: 'contents'
regex: "((\\\"|'|`)?((?i)aws)?_?((?i)access)_?((?i)key)?_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?(A3T[A-Z0-9]|AKIA|AGPA|AIDA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\\"|'|`)?)"
name: 'AWS Access Key ID'
- part: 'contents'
regex: "((\\\"|'|`)?((?i)aws)?_?((?i)account)_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[0-9]{4}-?[0-9]{4}-?[0-9]{4}(\\\"|'|`)?)"
name: 'AWS Account ID'
- part: 'contents'
regex: "((\\\"|'|`)?((?i)aws)?_?((?i)secret)_?((?i)access)?_?((?i)key)?_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[A-Za-z0-9/+=]{40}(\\\"|'|`)?)"
name: 'AWS Secret Access Key'
- part: 'contents'
regex: "((\\\"|'|`)?((?i)aws)?_?((?i)session)?_?((?i)token)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[A-Za-z0-9/+=]{16,}(\\\"|'|`)?)"
name: 'AWS Session Token'
- part: 'contents'
regex: "(?i)artifactory.{0,50}(\\\"|'|`)?[a-zA-Z0-9=]{112}(\\\"|'|`)?"
name: 'Artifactory'
- part: 'contents'
regex: "(?i)codeclima.{0,50}(\\\"|'|`)?[0-9a-f]{64}(\\\"|'|`)?"
name: 'CodeClimate'
- part:  'contents'
regex: 'EAACEdEose0cBA[0-9A-Za-z]+'
name: 'Facebook access token'
- part: 'contents'
regex: "((\\\"|'|`)?type(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?service_account(\\\"|'|`)?,?)"
name: 'Google (GCM) Service account'
- part:  'contents'
regex: '(?:r|s)k_[live|test]_[0-9a-zA-Z]{24}'
name: 'Stripe API key'
- part:  'contents'
regex: '[0-9]+-[0-9A-Za-z_]{32}\.apps\.googleusercontent\.com'
name: 'Google OAuth Key'
- part: 'contents'
regex: 'AIza[0-9A-Za-z\\-_]{35}'
name: 'Google Cloud API Key'
- part: 'contents'
regex: 'ya29\\.[0-9A-Za-z\\-_]+'
name: 'Google OAuth Access Token'
- part:  'contents'
regex: 'sk_[live|test]_[0-9a-z]{32}'
name: 'Picatic API key'
- part:  'contents'
regex: 'sq0atp-[0-9A-Za-z\-_]{22}'
name: 'Square Access Token'
- part:  'contents'
regex: 'sq0csp-[0-9A-Za-z\-_]{43}'
name: 'Square OAuth Secret'
- part:  'contents'
regex: 'access_token\$production\$[0-9a-z]{16}\$[0-9a-f]{32}'
name: 'PayPal/Braintree Access Token'
- part:  'contents'
regex: 'amzn\.mws\.[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}'
name: 'Amazon MWS Auth Token'
- part:  'contents'
regex: 'SK[0-9a-fA-F]{32}'
name: 'Twilo API Key'
- part:  'contents'
regex: 'key-[0-9a-zA-Z]{32}'
name: 'MailGun API Key'
- part:  'contents'
regex: '[0-9a-f]{32}-us[0-9]{12}'
name: 'MailChimp API Key'
- part:  'contents'
regex: "sshpass -p.*['|\\\"]"
name: 'SSH Password'
- part: 'contents'
regex: '(https\\://outlook\\.office.com/webhook/[0-9a-f-]{36}\\@)'
name: 'Outlook team'
- part: 'contents'
regex: "(?i)sauce.{0,50}(\\\"|'|`)?[0-9a-f-]{36}(\\\"|'|`)?"
name: 'Sauce Token'
- part: 'contents'
regex: '(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})'
name: 'Slack Token'
- part: 'contents'
regex: 'https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}'
name: 'Slack Webhook'
- part: 'contents'
regex: "(?i)sonar.{0,50}(\\\"|'|`)?[0-9a-f]{40}(\\\"|'|`)?"
name: 'SonarQube Docs API Key'
- part: 'contents'
regex: "(?i)hockey.{0,50}(\\\"|'|`)?[0-9a-f]{32}(\\\"|'|`)?"
name: 'HockeyApp'
- part: 'contents'
regex: '([\w+]{1,24})(://)([^$<]{1})([^\s";]{1,}):([^$<]{1})([^\s";]{1,})@[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,24}([^\s]+)'
name: 'Username and password in URI'
- part: 'contents'
regex: 'oy2[a-z0-9]{43}'
name: 'NuGet API Key'

*/
    }
}