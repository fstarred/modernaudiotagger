Ultimate Music Tagger
by Fabrizio Stellato
http://umtagger.codeplex.com/

Possible usage for command line:

tag single file: UltimateMp3TaggerShell.exe tag -file=<file> [options]
tag files in folder: UltimateMp3TaggerShell.exe tag -path=<folder> [options]
rename single file: UltimateMp3TaggerShell.exe rename -file=<file> -pattern=<pattern>
rename files in folder: UltimateMp3TaggerShell.exe rename -path=<folder> -pattern=<pattern>
[ tag mode ]

mode=<<manual|lastfm>>

use lastfm to retrieve tag information directly from lastfm remote services. 
Internet connection required. Edit umtagger.ini file to configure connection if behind proxy.

t|tagmode=<<a t T r R y p i g m>>

select which tag field to overwrite.
a = track/album artist
r = album
R = album artist
t = title
T = track artist
y = year
p= position / track
i = front cover image
g = genres
m = musicbrainzId (only with lastfm mode)
ALL = equivalent of atrpigm

[manual mode]
artist=<<artist>> artist
album=<<album>> album
title=<<title>> title
year=<<year>> year (ex. 2013)
image=<<image>> a valid image path
genres=<<genres>> genres comma separated (ex. rock, hard rock)
pos=<<position>> track position
[lastfm mode]
match=<<corrispondence>> track - filename match mode, valid values are n or p
[manual/lastfm mode]
ext=<<file extension>> file extension to process, other will be ignored. default extension is .mp3

about track / filename match

this option is used when using lastfm and path mode, in order to achieve which file is relative to the track's album returned by lastfm.

Just some examples:

using lastfm and path mode, search for artist:"Slayer" and album:"Reign in Blood" will return a list of tracks:

01 - Angel of Death
02 - Piece by piece
03 - Necrophobic
...

Ultimate Music Tagger will find a corrispondence match between each track and the filename found on the path.

n value 
file / track corrispondence is given by the name of the file and the track title
for example, "Angel Death.mp3" file be tagged as the first track, "Necrofobic" as the third and so on.

p value
file / track corrispondence is given by the number within the filename and the track position
for example, file "Track 02.mp3" or "-2 bla bla.mp3" is tagged as the second track.
It's important that the position is placed at the beginning or the end of the filename.

generally speaking, use n mode when filename is mostly like song title; 
use p mode when having filename with "Track 01" "Track 02" under working folder.
[ rename mode ]

pattern=<<pattern>> rename file(s) with specified pattern.

song title = %t %title
artist = %a %artist
album = %r %album
year = %y %d %year
track position =%p %pos

some examples:

"%p - %t" = "01 - Taxman.mp3"
"%a %r - %t %y" = "The Beatles Revolver - Taxman 1995.mp3"
"foo %t %p" = "foo Taxman 01.mp3"

Performing Lastfm search

be accurate as possible when performing lastfm search.

For example, doing a search with:

artist:Beatles , title:Walrus
or
artist:Beatles , title:The Walrus

return no valid track info.

This is because correct title to perform search is:
"I am The Walrus"

connection behind proxy

edit umtagger.ini file under executable folder to edit proxy settings.