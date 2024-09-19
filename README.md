# Snaffler 

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/T6T31VEVJ)

![A dictionary definition of "snaffle".](./snaffler.png)

## What is it for? 

Snaffler is a tool for **pentesters** and **red teamers** to help find delicious candy needles (creds mostly, but it's flexible) in a bunch of horrible boring haystacks (a massive Windows/AD environment).

It might also be useful for other people doing other stuff, but it is explicitly NOT meant to be an "audit" tool.

## I don't want to read all this!!!

Ugh, fine. But we aren't responsible for the results. We wrote all this other stuff for you, but that's okay. We're not mad, just disappointed.

`snaffler.exe -s -o snaffler.log`

## What does it do?

*Broadly speaking* - it gets a list of Windows computers from Active Directory, then spreads out its snaffly appendages to them all to figure out which ones have file shares, and whether you can read them. 

Then YET MORE snaffly appendages enumerate all the files in those shares and use **L**EARNED **A**RTIFACTUAL **I**NTELLIGENCE for **M**ACHINES to figure out which ones a grubby little hacker like you might want. 

Actually it doesn't do any ML stuff, because doing that right would require training data, and that would require an enormous amount of time that we don't have. Instead, like all good "ML" projects, it just uses a shitload of `if` statements and regexen.

## What does it look like?

Like this!

<p align="center">
  <img src="./snaffler_screenshot.png">
</p>

## How do I use it?

If you "literally just run the EXE on a domain joined machine in the context of a domain user" (as people were instructed to do with Grouper2, immediately before they ran it with all the verbose/debug switches on so it screamed several hundred megabytes of stack traces at them) it will basically do nothing. This is our idea of a prank<sup>TM</sup> on people who don't read README files, because we're monsters.

HOWEVER... if you add the correct incantations, it will enable the aforementioned L.A.I.M. and the file paths where candy may be found will fall out. 

The key incantations are: 

`-o`   Enables outputting results to a file. You probably want this if you're not using `-s`. e.g. `-o C:\users\thing\snaffler.log`

`-s`   Enables outputting results to stdout as soon as they're found. You probably want this if you're not using `-o`.

`-v`   Controls verbosity level, options are Trace (most verbose), Debug (less verbose, less gubs), Info (less verbose still, default), and Data (results only). e.g `-v debug` 

`-m`   Enables and assigns an output dir for snaffler to automatically take a copy of (or Snaffle... if you will) any found files that it likes.

`-l`   Maximum size of files (in bytes) to Snaffle. Defaults to 10000000, which is *about* 10MB.

`-i`   Disables computer and share discovery, requires a path to a directory in which to perform file discovery.

`-n`   Disables computer discovery, takes a comma-separated list of hosts to do share and file discovery on.

`-y`   TSV-formats the output.

`-b`   Skips the LAIM rules that will find less-interesting stuff, tune it with a number between 0 and 3.

`-f`   Limits Snaffler to finding file shares via DFS (Distributed File System) - this should be quite a bit sneakier than the default while still covering the biggest file shares in a lot of orgs.

`-a`   Skips file enumeration, just gives you a list of listable shares on the target hosts.

`-u`   Makes Snaffler pull a list of account names from AD, choose the ones that look most-interesting, and then use them in a search rule.

`-d`   Domain to search for computers to search for shares on to search for files in. Easy.

`-c`   Domain controller to query for the list of domain computers.

`-r`   The maximum size file (in bytes) to search inside for interesting strings. Defaults to 500k.

`-j`   How many bytes of context either side of found strings in files to show, e.g. `-j 200`

`-z`   Path to a config file that defines all of the above, and much much more! See below for more details. Give it `-z generate` to generate a sample config file called `.\default.toml`.

`-t` Type of log you would like to output. Currently supported options are plain and JSON. Defaults to plain.

`-x` Max number of threads to use. Don't set it below 4 or shit will break.

`-p` Path to a directory full of .toml formatted rules. Snaffler will load all of these in place of the default ruleset.

## What does any of this log output mean?

Hopefully this annotated example will help:

<p align="center">
  <img src="./log_key.png">
</p>

This log entry should be read roughly from left to right as:

* at 7:37ish
* Snaffler found a file it thinks is worth your attention
* it's rated it "Red", the second most-interesting level
* it matched a rule named "KeepConfigRegexRed"
* you can read it, but not modify it
* the exact regex that was matched is that stuff in the red box
* it's 208kB
* it was last modified on January 10th 2020 at quarter-to-four in the afternoon.
* the file may be found at the path in purple

... and the rest of the line (in grey) is a little snippet of context from the file where the match was.

In this case we've found ASP.NET validationKey and decryptionKey values, which might let us RCE the web app via some deserialisation hackery. Hooray!

Note: after this screenshot was made, Sh3r4 added a thing to prepend the current user and hostname to each line. I don't wanna redo the screenshot tho.

## How does it decide which files are good and which files are boring?

### The "so simple it's almost a lie" answer:
Each L.A.I.M. magic file finding method does stuff like:

* Searching by exact file extension match, meaning that any file with an extension that matches the relevant wordlist will be returned. This is meant for **file extensions** that are almost always going to contain candy, e.g. `.kdbx`, `.vmdk`, `.ppk`, etc.

* Searching by (case insensitive) exact filename match. This is meant for **file names** that are almost always going to contain candy, e.g. `id_rsa`, `shadow`, `NTDS.DIT`, etc.

* Searching by exact file extension match (yet another wordlist) FOLLOWED BY 'grepping' the contents of any matching files for certain key words (yet yet another another wordlist). This is meant for file extensions that **sometimes** contain candy but where you know there's likely to be a bunch of chaff to sift through. For example, `web.config` will sometimes contain database credentials, but will also often contain boring IIS config nonsense and no passwords. This will (for example) find anything ending in `.config`, then will grep through it for strings including but not limited to: `connectionString`, `password`, `PRIVATE KEY`, etc.

* Searching by partial filename match (oh god more wordlists). This is mostly meant to find `Jeff's Password File 2019 (Copy).docx` or `Privileged Access Management System Design - As-Built.docx` or whatever, by matching any file where the name contains the substrings `passw`, `handover`, `secret`, `secure`, `as-built`, etc.

* There's also skip-lists to skip all files with certain extensions, or any file with a path containing a given string.

### The real answer:

Snaffler uses a system of "classifiers", each of which examine shares or folders or files or file contents, passing some items downstream to the next classifier, and discarding others. Each classifier uses a set of rules to decide what to do with the items it classifies.

These rules can be very simple, e.g. "if a file's extension is `.kdbx`, tell me about it", or "if a path contains `windows\sxs` then stop looking at subdirectories and files within that path".

Rules can also use regular expressions, which allow for relatively sophisticated pattern-matching. This is particularly useful when examining file contents, although care should be taken to avoid regexen with a significant performance hit. In large environments these rules may be checked literally millions of times, so minor performance issues can be amplified significantly.

The real power is in Snaffler's ability to chain multiple rules together, and even create branching chains. This allows us to use "cheap" rules like checking file names and extensions to decide when to use "expensive" rules like running regexen across the contents of files, parsing certs to see whether they contain private keys, etc. This is what allows Snaffler to achieve quite deep inspection of files where needed, while also being surprisingly fast for a tool written in a higher-level language like C#.

For example, a very simple ruleset might contain:
* a rule to discard all files with extensions associated with image files
* a rule to find all files with the `.dmp` file extension and snaffle them
* a rule chain where:
  * the first rule looks for files with the `.ps1` file extension, and sends all matching files to both the second and third rules.
  * the second rule looks inside files using regexen designed to find hard-coded credentials in PowerShell code.
  * the third rule looks inside files using regexen designed to find hard-coded credentials in `cmd.exe` commands, as might be found in `.bat` or `.cmd` files, as these are also commonly used within PowerShell scripts.

This approach also lets us maintain a relatively manageable and legible ruleset, and also makes it much easier for the end-user (you) to customise the defaults or develop your own rulesets.

### I don't want to write rules, that sounds hard and boring.

You're right, it was.

Snaffler comes with a set of default rules baked into the `.exe`. You can see them in `./Snaffler/SnaffRules/DefaultRules`.

### I am a mighty titan of tedium, a master of the mundane, I wish to write my own ruleset.

No problem, you enormous weirdo. You have 2 options.

1. Edit or replace the rules in the `DefaultRules` directory, then build a fresh Snaffler. The `.toml` files in that dir will get baked into the `.exe` as resources, and loaded up at runtime whenever you don't specify any other rules to use.
2. Make a directory and stick a bunch of your own rule files in there, then run Snaffler with `-p .\path\to\rules`. Snaffler will parse all the `.toml` files in that directory and use the resulting ruleset. This will also work if you just have them all in one big `.toml` file.

Here's some annotated examples that will hopefully help to explain things better. If this seems very hard, you can just use our rules and they'll probably find you some good stuff.

This is an example of a rule that will make Snaffler ignore all files and subdirectories below a dir with a certain name.

```toml
[[ClassifierRules]]
EnumerationScope = "DirectoryEnumeration" # This defines which phase of the discovery process we're going to apply the rule. 
                                          # In this case, we're looking at directories. 
                                          # Valid values include ShareEnumeration, DirectoryEnumeration, FileEnumeration, ContentsEnumeration
RuleName = "DiscardLargeFalsePosDirs" # This can be whatever you want. We've been following a rough naming scheme, but you can call it "Stinky" if you want. ¯\_(ツ)_/¯
MatchAction = "Discard"# What to do with things that match the rule. In this case, we want to discard anything that matches this rule.
                        # Valid options include: Snaffle (keep), Discard, Relay (example of this below), and CheckForKeys (example below)
Description = "File paths that will be skipped entirely." # Not used in the code, just a place for notes really.
MatchLocation = "FilePath" # What part of the file/dir/share to look at to check for a match. In this case we're looking at the whole path.
                           # Valid options include: ShareName, FilePath, FileName, FileExtension, FileContentAsString, FileContentAsBytes,
                           # although obviously not all of these will apply in all EnumerationScopes.
WordListType = "Contains" # What matching logic to apply, valid options are: Exact, Contains, EndsWith, StartsWith, or Regex.
                          # Under the hood these all get turned into regexen one way or another.
MatchLength = 0
WordList = [ 
  # A list of strings or regex patterns to use to match. If using regex patterns, WordListType must be Regex.
	"\\\\puppet\\\\share\\\\doc",
	"\\\\lib\\\\ruby",
	"\\\\lib\\\\site-packages",
	"\\\\usr\\\\share\\\\doc",
	"node_modules",
	"vendor\\\\bundle",
	"vendor\\\\cache",
	"\\\\doc\\\\openssl",
	"Anaconda3\\\\Lib\\\\test",
	"WindowsPowerShell\\\\Modules",
	"Python27\\\\Lib"
]
Triage = "Green" # If we find a match, what severity rating should we give it. Valid values are Black, Red, Yellow, Green. This value is ignored for Discard MatchActions.
```

This rule on the other hand will look at file extensions, and immediately discard any we don't like.

In this case I'm mostly throwing away fonts, images, CSS, etc.
```toml
[[ClassifierRules]]
EnumerationScope = "FileEnumeration" # We're looking at the actual files, not the shares or dirs or whatever.
RuleName = "DiscardExtExact" # just a name
MatchAction = "Discard" # We're discarding these
MatchLocation = "FileExtension" # This time we're only looking at the file extension part of the file's name.
WordListType = "Exact" # and we only want exact matches. 
WordList = [".bmp", ".eps", ".gif", ".ico", ".jfi", ".jfif", ".jif", ".jpe", ".jpeg", ".jpg", ".png", ".psd", ".svg", ".tif", ".tiff", ".webp", ".xcf", ".ttf", ".otf", ".lock", ".css", ".less"] # list of file extensions.
```

Here's an example of a really simple rule for stuff we like and want to keep.
```toml
[[ClassifierRules]]
EnumerationScope = "FileEnumeration" # Still looking at files
RuleName = "KeepExtExactBlack" # Just a name
MatchAction = "Snaffle" # This time we are 'snaffling' these. This usually just means send it to the output, 
                       # but if you turn on the appropriate option it will also grab a copy.
MatchLocation = "FileExtension" # We're looking at file extensions again
WordListType = "Exact" # With Exact Matches
WordList = [".kdbx", ".kdb", ".ppk", ".vmdk", ".vhdx", ".ova", ".ovf", ".psafe3", ".cscfg", ".kwallet", ".tblk", ".ovpn", ".mdf", ".sdf", ".sqldump"] # and a bunch of fun file extensions.
Triage = "Black" # these are all big wins if we find them, so we're giving them the most severe rating.
```

This one is basically the same, but we're looking at the whole file name. Simple!
```toml
[[ClassifierRules]]
EnumerationScope = "FileEnumeration"
RuleName = "KeepFilenameExactBlack"
MatchAction = "Snaffle"
MatchLocation = "FileName"
WordListType = "Exact"
WordList = ["id_rsa", "id_dsa", "NTDS.DIT", "shadow", "pwd.db", "passwd"]
Triage = "Black"
```

This one is a bit nifty, check this out...
```toml
[[ClassifierRules]]
EnumerationScope = "FileEnumeration" # we're looking for files...
RuleName = "KeepCertContainsPrivKeyRed" 
MatchLocation = "FileExtension" # specifically, ones with certain file extensions...
WordListType = "Exact"
WordList = [".der", ".pfx"] # specifically these ones...
MatchAction = "CheckForKeys" # and any that we find, we're going to parse them as x509 certs, and see if the file includes a private key!
Triage = "Red" # cert files aren't very sexy, and you'll get huge numbers of them in most wintel environments, but this check gives us a way better SNR!
```

OK, here's where the powerful stuff comes in. We got a pair of rules in a chain here.

Files with extensions that match the first rule will be sent to second rule, which will "grep" (i.e. String.Contains()) them for stuff in a specific wordlist. 

You can chain these together as much as you like, although I imagine you'll start to see some performance problems if you get too inception-y with it.
```toml
[[ClassifierRules]]
EnumerationScope = "FileEnumeration" # this one looks at files...
RuleName = "ConfigGrepExtExact"
MatchLocation = "FileExtension" # specifically the extensions...
WordListType = "Exact"
WordList = [".yaml", ".xml", ".json", ".config", ".ini", ".inf", ".cnf", ".conf"] # these ones.
MatchAction = "Relay" # Then any files that match are handed downstream...
RelayTargets = ["KeepConfigGrepContainsRed"] # To the rule with this RuleName! This can also be an array of RuleNames if you want to get real wild and start writing branching rulesets.

[[ClassifierRules]]
RuleName = "KeepConfigGrepContainsRed" # Anyway, this is the target rule. Following a naming convention really helps to make sure you're using the right targets.
EnumerationScope = "ContentsEnumeration" # this one looks at file content!
MatchAction = "Snaffle" # it keeps files that match
MatchLocation = "FileContentAsString" # it's looking at the contents as a string (rather than a byte array)
WordListType = "Contains" # it's using simple matching
WordList = ["password=", " connectionString=\"", "sqlConnectionString=\"", "validationKey=", "decryptionKey=", "NVRAM config last updated"]
Triage = "Red"
```

Hopefully this convey the idea. I'd recommend taking some of the default rules and tinkering with them until you feel like you've got a good handle on it.

## WTF is an "UltraSnaffler"???

A lot of people wanted the ability to look inside file formats that weren't just flat text, like Word documents, PDFs, `.eml`, etc. Unfortunately, the easiest library for implementing that functionality blew out the final file size on `Snaffler.exe` by about 1200%, which sucked for a bunch of the popular in-memory execution techniques that had upper limits on how big a file they could be used with.

The solution was UltraSnaffler, which is just a second `.sln` file that enables the required lib and the relevant code. Build `UltraSnaffler.sln`, get UltraSnaffler.

WARNING: Snaffler's default rules don't include any that will look inside Office docs or PDFs, because we found it really difficult to write any that weren't going to just take *years* to finish a run in a typical corporate environment. *Be warned, looking inside these docs is a lot slower than looking inside good old fashioned text files, and a typical environment will have an absolute mountain of low-value Office docs and PDFs.*

## How does the config file thing work?

This is actually really neat IMO.

If you add `-z generate` onto the end of a Snaffler command line, Snaffler will serialise the configuration object (including whatever aspects of the configuration were set by your args) into a `.toml` config file, which you can then hand-edit pretty easily (or not) and then re-use at your leisure

For example, if you do:

`Snaffler.exe -s -o C:\mydir\snaffler.log -v trace -i \\host.lol.domain\share -p C:\users\someguy\myrules -z generate`

Snaffler will parse all your many, many arguments, turn them into a config object, serialise that config object into the following `.toml` config file:

```toml
PathTargets = ["\\\\host.lol.domain\\share"]
ComputerTargetsLdapFilter = "(objectClass=computer)"
ScanSysvol = true
ScanNetlogon = true
ScanFoundShares = true
InterestLevel = 0
DfsOnly = false
DfsShareDiscovery = false
DfsNamespacePaths = []
CurrentUser = "l0sslab\\l0ss"
RuleDir = "C:\\users\\someguy\\myrules"
MaxThreads = 60
ShareThreads = 20
TreeThreads = 20
FileThreads = 20
MaxFileQueue = 200000
MaxTreeQueue = 0
MaxShareQueue = 0
LogToFile = true
LogFilePath = "C:\\mydir\\snaffler.log"
LogType = "Plain"
LogTSV = false
Separator = 32
LogToConsole = true
LogLevelString = "trace"
ShareFinderEnabled = false
LogDeniedShares = false
DomainUserRules = false
DomainUserMinLen = 6
DomainUserNameFormats = ["sAMAccountName"]
DomainUserMatchStrings = ["sql", "svc", "service", "backup", "ccm", "scom", "opsmgr", "adm", "adcs", "MSOL", "adsync", "thycotic", "secretserver", "cyberark", "configmgr"]
DomainUsersWordlistRules = ["KeepConfigRegexRed"]
MaxSizeToGrep = 1000000
Snaffle = false
MaxSizeToSnaffle = 10000000
MatchContextBytes = 200
```

You may notice that there are many items in here that you didn't pass arguments for. Those values are the default config items, some of which can only be edited easily in the source or via a config file, usually because it didn't seem worth it to add an argument for them.

## This sucks, do you have plans to make it suck less?

No it doesn't, you suck.

Also, yes we do.

We're also going to: 
 - Add parsing of archive files, ideally treating them as just another dir to walk through looking for goodies.
 - Keep refining the rules and regexen. **More words for the wordlists! `string[]`s for the `string` throne!**

![A dumb joke about wordlists.](./WORDLISTS.png)

## Who did you steal code from?

The share enumeration bits were snaffled (see what I did there?) from SharpShares, which was written by the exceedingly useful Dwight Hohnstein. (https://github.com/djhohnstein/SharpShares/)
Dwight's GitHub profile is like that amazing back aisle at a hardware store that has a whole bunch of tools that make you go "oh man I can't wait til I have an excuse to try this one for realsies..." and you should definitely check it out.

While no code was taken (mainly cos it's Ruby lol) we did steal a bunch of nifty ideas from `plunder2` (http://joshstone.us/plunder2/)

Wordlists were also curated from those found in some other similar-ish tools like trufflehog, shhgit, gitrobber, and graudit.


## Is it OPSEC safe? (Whatever the hell that means)

Pffft, no. It's noisy as fuck.

Look let's put it this way... If it's the kind of environment where you'd feel confident running BloodHound in its default mode, then uhhh, yeah man... It's real stealthy.

## I thought you used this thing on red team gigs?

*sigh* 

OK, I'll give you the real answer.

In default mode, Snaffler looks an awful lot like SharpHound, in a lot of ways. It talks a bunch of LDAP to AD, then it goes out and tries to talk SMB to every Windows machine in the domain. This kind of behaviour is pretty much guaranteed to get you busted in an org that has their shit even slightly together.

HOWEVER...

Snaffler's more-targeted options (especially `-i`) are a *lot* less likely to trigger detections. 

I am particularly fond of running `Snaffler.exe -s -i C:\` on a freshly compromised server or workstation, and I've not seen this behaviour get detected. 

Yet.

## How can I help or get help?

If you want to discuss via Slack you can ping us (@l0ss or @Sh3r4) on the BloodHound Slack, joinable at https://bloodhoundgang.herokuapp.com/, or chat with a group of contributors in the #snaffler channel.

You can also ping us on Twitter - @mikeloss and @sh3r4_hax

Otherwise file an issue; we'll try.

