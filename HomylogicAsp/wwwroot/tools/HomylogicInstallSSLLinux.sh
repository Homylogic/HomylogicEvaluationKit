if ! [ $(id -u) = 0 ]; then
   echo "The script need to be run as root." >&2
   exit 1
fi

wget --output-document homylogic.crt http://homylogic.go/Home/Download?file=ssl/homylogic.crt

cp homylogic.crt /usr/local/share/ca-certificates/homylogic.crt

if [ $( ls homylogic.crt ) ]; then 
   rm homylogic.crt; 
fi

update-ca-certificates