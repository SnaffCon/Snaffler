# Snaffler 

![A dictionary definition of "snaffle".](./snaffler.png)

## What is it for? 

Snaffler is a tool for **pentesters** to help find delicious candy needles (creds mostly, but it's flexible) in a bunch of horrible boring haystacks (a massive Windows/AD environment).

It might also be useful for other people doing other stuff, but it is explicitly NOT meant to be an "audit" tool.

## I don't want to read all this!!!

Ugh, fine. But we aren't responsible for the results. We wrote all this other stuff for you, but that's okay. We're not mad, just disappointed.

`snaffler.exe  -s -o snaffler.log`

## What does it do?

It gets a list of Windows computers from Active Directory, then spreads out its snaffly appendages to them all to figure out which ones have file shares, and whether you can read them. 

Then YET MORE snaffly appendages enumerate all the files in those shares and use **L**EARNED **A**RTIFACTUAL **I**NTELLIGENCE for **M**ACHINES to figure out which ones a grubby little hacker like you might want. 

Actually it doesn't do any ML stuff (yet), because doing that right would require training data, and that would require an enormous amount of time that we don't have.

## What does it look like?

Like this (mostly)!

<p align="center">
  <img src="./snaffler_screenshot.png">
</p>

## How do I use it?

If you "literally just run the EXE on a domain joined machine in the context of a domain user" (as people were instructed to do with Grouper2, immediately before they ran it with all the verbose/debug switches on so it screamed several hundred megabytes of stack traces at them) it will basically do nothing. This is our idea of a prank<sup>TM</sup> on people who don't read README files, because we're monsters.

HOWEVER... if you add the correct incantations, it will enable the aforementioned L.A.I.M. and the file paths where candy may be found will fall out. 

The key incantations are: 

`-o`   Enables outputting results to a file. You probably want this if you're not using -s. e.g. `-o C:\users\thing\snaffler.log`

`-s`   Enables outputting results to stdout as soon as they're found. You probably want this if you're not using -o.

`-v`   Controls verbosity level, options are Trace (most verbose), Debug (less verbose), Info (less verbose still, default), and Data (results only). e.g `-v debug` 

`-m`   Enables and assigns an output dir for snaffler to automatically take a copy of any found files that it likes.

`-f`   Disables file discovery, will only perform computer and share discovery.

`-i`   Disables computer and share discovery, requires a path to a directory in which to perform file discovery.

`-t`   Maximum number of threads. Default 30.

`-d`   Domain to search for computers to search for shares on to search for files in. Easy.

`-a`   Enables scanning of C$ shares if found. Can be prone to false positives but more thorough. Snaffler will still tell you that it FOUND these shares even if it doesn't look inside them.

`-c`   Domain controller to query for the list of domain computers.

`-r`   The maximum size file (in bytes) to search inside for interesting strings. Defaults to 500k.

`-j`   How many bytes of context either side of found strings in files to show, e.g. `-j 200`


## How does it decide which files are good and which files are boring?

Each L.A.I.M. magic file finding method has its own separate wordlist (in SnaffCore/Config ) that you can edit yourself.

* Searching by exact file extension match, meaning that any file with an extension that matches the relevant wordlist will be returned. This is meant for **file extensions** that are almost always going to contain candy, e.g. `.kdbx`, `.vmdk`, `.ppk`, etc.

 - Searching by (case insensitive) exact filename match. This is meant for **file names** that are almost always going to contain candy, e.g. `id_rsa`, `shadow`, `NTDS.DIT`, etc.

 - Searching by exact file extension match (yet another wordlist) FOLLOWED BY 'grepping' the contents of any matching files for certain key words (yet yet another another wordlist). This is meant for file extensions that **sometimes** contain candy but where you know there's likely to be a bunch of chaff to sift through. For example, `web.config` will sometimes contain database credentials, but will also often contain boring IIS config nonsense and no passwords. This will (for example) find anything ending in `.config`, then will grep through it for strings including but not limited to: `connectionString`, `password`, `PRIVATE KEY`, etc.

 - Searching by partial filename match (oh god more wordlists). This is mostly meant to find `Jeff's Password File 2019 (Copy).docx` or `Privileged Access Management System Design - As-Built.docx` or whatever, by matching any file where the name contains the substrings `passw`, `handover`, `secret`, `secure`, `as-built`, and a few others.

 - There's also a couple of skip-lists you can use to have it skip all files with certain extensions, or any file with a path containing a given string.

## This sucks, do you have plans to make it suck less?

Very much so.

Next step on the roadmap is a bunch of extra concurrency magic to make everything even more fasterer, and to allow us to define more complex multi-step classification of files. 

For example: 
 - "find all files ending in `.config`, grep inside them for string X, then grep inside the ones that match for string Y" 
 or 
 - "find all files ending in `.der`, then parse them as x509 certs to see if they have keys inside.

This will help cut down the false positive rate somewhat.

We're also going to: 
 - Add a basic triage system to flag up the really sexy findings much more visibly.
 - Add a bunch of regex stuff to help cut down on false positives even more.
 - Add parsing of MS Word and Excel documents.
 - Add even more things we should be searching for inside files, like db connection strings from various languages. **More words for the wordlists! `string[]`s for the `string` throne!**

![A dumb joke about wordlists.](./WORDLISTS.png)

## Who did you steal code from?

The share enumeration bits were snaffled (see what I did there?) from SharpShares, which was written by the exceedingly useful Dwight Hohnstein. (https://github.com/djhohnstein/SharpShares/)
Dwight's GitHub profile is like that amazing back aisle at a hardware store that has a whole bunch of tools that make you go "oh man I can't wait til I have an excuse to try this one for realsies..." and you should definitely check it out.

While no code was taken (mainly cos it's Ruby lol) we did steal a bunch of nifty ideas from `plunder2` (http://joshstone.us/plunder2/)

Wordlists were also curated from those found in some other similar-ish tools like trufflehog, gitrobber, and graudit.


## Is it OPSEC safe? (Whatever the hell that means)

Pffft, no. It's noisy as fuck.

Look let's put it this way... If it's the kind of environment where you'd feel confident running something like CrackMapExec, then uhhh, yeah man... It's real stealthy.


## How can I help or get help?

If you want to discuss via Slack you can ping us (@l0ss or @Sh3r4) on the BloodHound Slack, joinable at https://bloodhoundgang.herokuapp.com/, or chat with a group of contributors in the #snaffler channel.

You can also ping us on Twitter - @mikeloss and @sh3r4_hax

Otherwise file an issue; we'll try.
