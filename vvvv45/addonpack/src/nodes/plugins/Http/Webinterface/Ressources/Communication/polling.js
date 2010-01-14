    //Ask VVVV every second if there are new data from vvvv
	$(document).ready(function(){
        $(document).everyTime(900,function(i) {
		
           $.ajax({
            type: "POST",
            url: "polling.xml",
            data: 'Gib mir neue Daten',
            success: function(xml)
            {
                
				// checks if the page should be reloaded
//                if($(xml).find('Reload'))
//                {
//                    location.reload(true);
//                }
				
				// runs through the received xml to change the specific elements
				$(xml).find('node').each(function()
					{
						
						var ActualNode = $(this);
						
						// selectes the ObjectId and the ObjectName
						var objectId = $(ActualNode).attr("SliceId");
						var objectMethodName = $(ActualNode).attr("ObjectMethodName");
		
						//locks for alle Elements tags with icludes the values to set 
						var ParametersArray = new Array();					
						$(ActualNode).find("MethodParameters").each(function()
						{
						
							var element = $(this).text();
							ParametersArray.push(element);
						
						});
					
						// depending on hte sizw of the array the options are set					
						switch(ParametersArray.length)
						{
							case 1: 
								$(objectId)[objectMethodName](ParametersArray[0]);
								break;
							case 2: 
								$(objectId)[objectMethodName](ParametersArray[0], ParametersArray[1]);
								break;
							case 3: 
								$(objectId)[objectMethodName](ParametersArray[0], ParametersArray[1],ParametersArray[2]);
								break;
							default:
								console.log("Error in handling vvvv polling data");
								break;
						}

					});
            }
        });
        
           
        }, 0);
    });
	
	// Händels the received xml data
    function processChunk(i){ 
        $.ajax({
            type: "POST",
            url: "polling.xml",
            data: 'Gib mir neue Daten',
            success: function(xml)
            {ooo
                
				// checks if the page should be reloaded
//                if($(xml).find('Reload'))
//                {
//                    location.reload(true);
//                }
				
				// runs through the received xml to change the specific elements
				$(xml).find('node').each(function()
					{
						
						var ActualNode = $(this);
						
						// selectes the ObjectId and the ObjectName
						var objectId = $(ActualNode).attr("SliceId");
						var objectName = $(ActualNode).attr("ObjectName");
		
						//locks for alle Elements tags with icludes the values to set 
						var ElemetsArray = new Array();					
						var Elements = $(ActualNode).find("Element").each(function()
						{
						
							var element = $(this).text();
							ElemetsArray.push(element);
						
						});
					
						// depending on hte sizw of the array the options are set					
						switch(ElemetsArray.length)
						{
							case 1: 
								$(objectId)[objectName](ElemetsArray[0]);
								break;
							case 2: 
								$(objectId)[objectName](ElemetsArray[0], ElemetsArray[1]);
								break;
							case 3: 
								$(objectId)[objectName](ElemetsArray[0], ElemetsArray[1], ElemetsArray[2]);
								break;
							default:
								console.log("Error in handling vvvv polling data");
								break;
						}

					});
            }
        });
    }; 	