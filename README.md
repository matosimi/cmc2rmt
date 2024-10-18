# cmc2rmt
Tool to convert Atari 8-bit CMC song into RMT song (song and patterns only, instruments are not being converted at all, you have to assign them on your own).

Go to https://asma.atari.org/asmadb/ , search for some song in CMC format and download it (for instance Lasermania).

Execute cmc2rmt.exe <filename.cmc>

You can redirect output to file following way: cmc2rmt.exe Lasermania.cmc > Lasermania.rmt.txt

Open RMT, File->Load... ,change filter to TXT song files (.txt) and load the export of cmc2rmt tool.

Continue editing the song in RMT.

