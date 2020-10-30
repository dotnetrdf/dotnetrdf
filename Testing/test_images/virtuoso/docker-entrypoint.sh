#!/bin/bash

# Exit on first error
set -e

#set NumberOfBuffers and MaxDirtyBuffers parameters in Virtuoso.ini
totalMem=$(cat /proc/meminfo | grep "MemTotal" | grep -o "[0-9]*")

virtMemAlloc=$(($totalMem/2))
nBuffers=$(($virtMemAlloc/9))
dirtyBuffers=$(($nBuffers*3/4))

#echo "Virtuoso params: NumberOfBuffers $nBuffers ; MaxDirtyBuffers: $dirtyBuffers "
sed -i "s/^\(NumberOfBuffers\s*= \)[0-9]*/\1$nBuffers/" /etc/virtuoso-opensource-7/virtuoso.ini
sed -i "s/^\(MaxDirtyBuffers\s*= \)[0-9]*/\1$dirtyBuffers/" /etc/virtuoso-opensource-7/virtuoso.ini

exec "$@"
