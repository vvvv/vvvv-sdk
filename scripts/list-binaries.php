<?php
// open the current directory by opendir
$handle=opendir(".");

while (($file = readdir($handle)) !== false) 
{
  if (pathinfo($file, PATHINFO_EXTENSION) == "md5")
  {
    echo "$file\n";
  }
}

closedir($handle);
?>