using Classifiers;
using System.Collections.Generic;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildFileNameRules()
        {

            /*
            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    Description = "Files with these extensions are very very interesting.",
                    RuleName = "KeepExtExactBlack",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Black,
                    WordList = new List<string>()
                    {
                        // keepass databases
                        ".kdbx", // test file created
                        ".kdb", // test file created
                        // putty private keys
                        ".ppk", // test file created
                        // virtual disks
                        ".vmdk", // test file created
                        ".vhd", // test file created
                        ".vhdx", // test file created
                        // password safe
                        ".psafe3", // test file created
                        // cloud service config
                        ".cscfg", // test file created
                        // kde wallet manager
                        ".kwallet", // test file created
                        // vpn profiles
                        ".tblk", // test file created
                        ".ovpn", // test file created
                    },
                });

            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    Description = "Files with these exact names are very very interesting.",
                    RuleName = "KeepFilenameExactBlack",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileName,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    WordList = new List<string>()
                    {
                        // these are file names that we always want to keep if it's an exact match.
                        "id_rsa", // test file created
                        "id_dsa", // test file created
                        "id_ecdsa", // test file created
                        "id_ed25519", // test file created
                        "NTDS.DIT", // test file created
                        "shadow", // test file created
                        "pwd.db", // test file created
                        "passwd", // test file created
                        "running-config.cfg", // test file created
                        "startup-config.cfg", // test file created
                        "running-config", // test file created
                        "startup-config" // test file created
                    },
                }
                );

            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    Description = "Files with a path containing these strings are very very interesting.",
                    RuleName = "KeepPathContainsBlack",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FilePath,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Black,
                    WordList = new List<string>()
                {
                        "\\.ssh\\", // test file created
                        ".purple\\accounts.xml", // test file created
                        ".aws\\", // test file created
                        ".gem\\credentials", // test file created
                        "doctl\\config.yaml", // test file created
                        "config\\hub"  // test file created
                },
                }
            );


            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    Description = "Files with these extensions are QUITE interesting.",
                    RuleName = "KeepExtExactRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        ".jks", // test file created
                        // rdp
                        ".rdp", // test file created
                        ".rdg", // test file created
                        // bitlocker recovery keys
                        ".bek", // test file created
                        // tpm backups
                        ".tpm", // test file created
                        ".fve", // test file created
                        // packet capture
                        ".pcap", // test file created
                        ".cap", // test file created
                        // misc key material
                        ".key", // test file created
                        ".keypair", // test file created
                        ".keychain", // test file created
                        // disk image
                        ".wim", // test file created
                        // virtual machines
                        ".ova", // test file created
                        ".ovf", // test file created
                        // db backups
                        ".mdf", // test file created
                        ".sdf", // test file created
                        ".sqldump" // test file created
                    },
                }
                );


            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    Description = "Files with these exact names are very interesting.",
                    RuleName = "KeepFilenameExactRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileName,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        "unattend.xml", // test file created
                        ".netrc", // test file created
                        "_netrc", // test file created
                        ".htpasswd", // test file created
                        "otr.private_key", // test file created
                        ".secret_token.rb", // test file created
                        "carrierwave.rb", // test file created
                        "database.yml", // test file created
                        "omniauth.rb", // test file created
                        "settings.py", // test file created
                        ".agilekeychain", // test file created
                        ".keychain", // test file created
                        "jenkins.plugins.publish_over_ssh.BapSshPublisherPlugin.xml", // test file created
                        "credentials.xml", // test file created
                        "LocalSettings.php", // test file created
                        "Favorites.plist", // test file created
                        "knife.rb", // test file created
                        "proxy.config", // test file created
                        "proftpdpasswd", // test file created
                        "robomongo.json", // test file created
                        "filezilla.xml", // test file created
                        "recentservers.xml", // test file created
                        "terraform.tfvars", // test file created
                        ".exports", // test file created
                        ".functions", // test file created
                        ".extra", // test file created
                        ".bash_history", // test file created
                        ".zsh_history", // test file created
                        ".sh_history", // test file created
                        "zhistory", // test file created
                        ".mysql_history", // test file created
                        ".psql_history", // test file created
                        ".pgpass", // test file created
                        ".irb_history", // test file created
                        ".dbeaver-data-sources.xml", // test file created
                        ".s3vfg", // test file created
                        "sftp-config.json", // test file created
                        "config.inc", // test file created
                        "config.php", // test file created
                        "keystore", // test file created
                        "keyring", // test file created
                        ".tugboat", // test file created
                        ".git-credentials", // test file created
                        ".gitconfig", // test file created
                        ".dockercfg", // test file created
                        ".npmrc", // test file created
                        ".env", // test file created
                        ".bashrc", // test file created
                        ".profile", // test file created
                        ".zshrc", // test file created
                    },
                }
                );

            /*
            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleOrder = 12,
                RuleName = "KeepNameContainsGreen",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileName,
                WordListType = MatchListType.Contains,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Green,
                WordList = new List<string>()
                    {
                        //magic words
                        "passw",
                        "as-built",
                        "handover",
                        "secret",
                        "thycotic",
                        "cyberark",
                    },
            });
            */

        }
    }
}
