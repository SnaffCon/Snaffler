using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildFileKeepClassifiers()
        {
            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    RuleOrder = 1,
                    RuleName = "KeepExtExactBlack",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Black,
                    WordList = new List<string>()
                    {
                        // keepass databases
                        ".kdbx",
                        ".kdb",
                        // putty private keys
                        ".ppk",
                        // virtual disks
                        ".vmdk",
                        ".vhdx",
                        // virtual machines
                        ".ova",
                        ".ovf",
                        // password safe
                        ".psafe3",
                        // cloud service config
                        ".cscfg",
                        // kde wallet manager
                        ".kwallet",
                        // vpn profiles
                        ".tblk",
                        ".ovpn",
                        // db backups
                        ".mdf",
                        ".sdf",
                        ".sqldump"
                    },
                });

            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    RuleOrder = 2,
                    RuleName = "KeepFilenameExactBlack",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileName,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    WordList = new List<string>()
                    {
                        // these are file names that we always want to keep if it's an exact match.
                        "id_rsa",
                        "id_dsa",
                        "NTDS.DIT",
                        "shadow",
                        "pwd.db",
                        "passwd",
                    },
                }
                );

                this.ClassifierRules.Add(
                    new ClassifierRule()
                    {
                        RuleOrder = 3,
                        RuleName = "KeepPathContainsBlack",
                        EnumerationScope = EnumerationScope.FileEnumeration,
                        MatchLocation = MatchLoc.FilePath,
                        WordListType = MatchListType.Contains,
                        MatchAction = MatchAction.Snaffle,
                        Triage = Triage.Black,
                        WordList = new List<string>()
                    {
                        ".ssh\\",
                        ".purple\\accounts.xml",
                        ".aws\\",
                        ".gem\\credentials",
                        "doctl\\config.yaml",
                        "config\\hub"
                    },
                    }
                );
            

            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    RuleOrder = 4,
                    RuleName = "KeepExtExactRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        // backups
                        ".bak",
                        // priv keys and certs
                        ".key",
                        ".pk12",
                        ".p12",
                        ".pkcs12",
                        //".pfx",
                        ".jks",
                        // rdp
                        ".rdp",
                        ".rdg",
                        // actionscript
                        ".asc",
                        // bitlocker recovery keys
                        ".bek",
                        // tpm backups
                        ".tpm",
                        ".fve",
                        // packet capture
                        ".pcap",
                        ".cap",
                        // misc key material
                        ".key",
                        ".keypair",
                        ".keychain",
                        // disk image
                        ".wim",
                    },
                }
                );
            

            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    RuleOrder = 5,
                    RuleName = "KeepFilenameExactRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileName,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        "unattend.xml",
                        ".netrc",
                        "_netrc",
                        ".htpasswd",
                        "otr.private_key",
                        ".secret_token.rb",
                        "carrierwave.rb",
                        "database.yml",
                        "omniauth.rb",
                        "settings.py",
                        ".agilekeychain",
                        ".keychain",
                        "jenkins.plugins.publish_over_ssh.BapSshPublisherPlugin.xml",
                        "credentials.xml",
                        "LocalSettings.php",
                        "Favorites.plist",
                        "knife.rb",
                        "proxy.config",
                        "proftpdpasswd",
                        "robomongo.json",
                        "filezilla.xml",
                        "recentservers.xml",
                        "terraform.tfvars",
                        ".exports",
                        ".functions",
                        ".extra",
                        ".bash_history",
                        ".zsh_history",
                        ".sh_history",
                        "zhistory",
                        ".mysql_history",
                        ".psql_history",
                        ".pgpass",
                        ".irb_history",
                        ".dbeaver-data-sources.xml",
                        ".s3vfg",
                        "sftp-config.json",
                        "config.inc",
                        "config.php",
                        "keystore",
                        "keyring",
                        ".tugboat",
                        ".git-credentials",
                        ".gitconfig",
                        ".dockercfg",
                        ".npmrc",
                        ".env",
                        ".bashrc",
                        ".profile",
                        ".zshrc",
                    },
                }
                );

            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    RuleOrder = 6,
                    RuleName = "KeepCertContainsPrivKeyRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.CheckForKeys,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        ".der",
                        ".pfx"
                    },
                }
                );
        }
    }
}
