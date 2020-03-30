# Snaffler 

![A dictionary definition of "snaffle".](./snaffler.png)

## What is it for? 

Snaffler is a tool for **pentesters** to help find delicious candy needles (creds mostly, but it's flexible) in a bunch of horrible boring haystacks (a massive Windows/AD environment).

It might also be useful for other people doing other stuff, but it is explicitly NOT meant to be an audit tool. If this README is starting to sound familiar then maybe you can guess how many times people have requested rewrites that are explicitly for blueteamers.

## What does it do?

It gets a list of Windows computers from Active Directory, then spreads out its snaffly appendages to them all to figure out which ones have file shares, and whether you can read them. Then YET MORE snaffly appendages enumerate all the files in those shares and use **L**EARNED **A**RTIFACTUAL **I**NTELLIGENCE for **M**ACHINES to figure out which ones a grubby little hacker like you might want.

## How do I use it?

If you "literally just run the EXE on a domain joined machine in the context of a domain user" (as people were instructed to do with Grouper, immediately before they ran it with all the verbose/debug switches on so it screamed several hundred megabytes of stack traces at them) it will find all the domain computers and shares, but it won't actually find you any files. This is our idea of a prank<sup>TM</sup> on people who don't read README files, because we're monsters.

HOWEVER... if you add the correct incantations, it will enable the aforementioned L.A.I.M. magic file finding methods of your choosing, and the file paths where candy may be found will fall out. 

Each L.A.I.M. magic file finding method has its own separate wordlist so you can run them all at once if you want. It will obviously be slower, but it should work just fine.

The key incantations are:

`-e` - Enables searching by exact file extension match, meaning that any file with an extension that matches the relevant wordlist will be returned. This is meant for **file extensions** that are almost always going to contain candy, e.g. `.kdbx`, `.vmdk`, `.ppk`, etc.

`-n` - Enables searching by (case insensitive) exact filename match. This is meant for **file names** that are almost always going to contain candy, e.g. `id_rsa`, `shadow`, `NTDS.DIT`, etc.

`-g` - Enables searching by exact file extension match (yet another wordlist) FOLLOWED BY grepping the contents of any matching files for certain key words (yet yet another another wordlist). This is meant for file extensions that **sometimes** contain candy but where you know there's likely to be a bunch of chaff to sift through. For example, `web.config` will sometimes contain database credentials, but will also often contain boring IIS config nonsense. This will find anything ending in `.config`, then will grep through them all for strings including but not limited to: `connectionString`, `password`, `PRIVATE KEY`, etc.

`-p` - Enables searching by partial filename match (oh god more wordlists). This is mostly meant to find `Jeff's Password File 2019 (Copy).docx` or `Privileged Access Management System Design - As-Built.docx` or whatever, by matching any file where the name contains the substrings `passw`, `handover`, `secret`, `secure`, `as-built`, and a few others.

## This sucks, do you have plans to make it suck less?

Yeah. In no particular order the plan includes:
 - Various file output formats, maybe `.json` just to tick Sh3r4 off.
 - Allow the user to define how many threads to use and then once all shares have been enumerated, automatically allocate those threads to finding files.
 - Prioritise computers and shares based on strings in computer and share names, e.g. `dev`, `sql`, `backup`, etc.
 - Implement the "Tarpit" feature from `plunder2` that lets the user skip a dir if it contains more than X number of files.
 - Maybe let the user manually point the file searchy thing at a directory and skip all the share stuff, so it can be used to sift goodies from local filesystems or in a more targeted way?
 - Implement the "Mirror" feature from `plunder2` which automatically copies files that meet certain criteria (mainly file size, but hmmm, maybe a wordlist...) to the user's local machine.
 - A pane-based interactive status display showing current stats, a scrolling list of files found, and ideally, the ability to interactively remove target hosts/shares that are bogging things down due to a slow link, a big weird filesystem, etc.
 - Allow the user to terminate and resume a previous run of the tool.
 - Try to come up with more things we should be searching for like db connection strings from various scripty/webby languages. All these things should be in our wordlists. **More words for the wordlists! `string[]`s for the `string` throne!**

![A dumb joke about wordlists.](./WORDLISTS.png)

## Who did you steal code from?

The share enumeration bits were snaffled (see what I did there?) from SharpShares, which was written by the exceedingly useful Dwight Hohnstein. (https://github.com/djhohnstein/SharpShares/)
Dwight's GitHub profile is like that amazing back aisle at a hardware store that has a whole bunch of tools that make you go "oh man I can't wait til I have an excuse to try this one for realsies..." and you should definitely check it out.

While no code was taken (mainly cos it's Ruby lol) we did steal a bunch of nifty ideas from `plunder2` (http://joshstone.us/plunder2/)


## Is it OPSEC safe? (Whatever the hell that means)

Pffft, no. It's noisy as fuck.

Look let's put it this way... If it's the kind of environment where you'd feel confident running something like CrackMapExec, then uhhh, yeah man... It's real stealthy.


## How can I help or get help?

If you want to discuss via Slack you can ping us (@l0ss or @Sh3r4) on the BloodHound Slack, joinable at https://bloodhoundgang.herokuapp.com/, or chat with a group of contributors in the #Grouper channel.

Otherwise file an issue; we'll try.
