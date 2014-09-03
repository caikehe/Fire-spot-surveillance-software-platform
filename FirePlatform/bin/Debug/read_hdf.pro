pro read_hdf,dir,image=image,Latitude_data=Latitude_data,Longitude_data=Longitude_data
   
   template =hdf_browser(dir) 
   ;在出现的对话框中选择hdf/netcdf,在下面的下拉列表中选择SD（varible/attribute） 
   ;这时便会出现很多的变量，选择你需要的那一个 
   ;在下面的attribute value中选择read选项，后面方框中便会出现读取后的变量，或者结构 
   ;变量中的成员名，可以修改名字,ok即可 
;   help,template 
   variable =tag_names(template);out结构的成员名 
;   print,variable 
   out =hdf_read(template =template) 
   num =n_tags(out);out结构的成员数 
   variable =tag_names(out);out结构的成员名 
;   print,variable 
   image =out.(3);在引用结构变量时，可用索引号来表示这里的out.(3)是第四个成员 
;   image1 =transpose(image) 
;   help,image1 
;   openw,lun,'d:\data.dat',/get_lun 
;   writeu,lun,image1 
;   free_lun,lun 
;   write_tiff,'d:\ab.tif',image,/float 


 ; Set some constants
 Latitude_SDS_NAME1="Latitude";纬度
 Latitude_SDS_NAME2="FP_latitude"
 Longitude_SDS_NAME1="Longitude";经度
 Longitude_SDS_NAME2="FP_longitude"
 
 ; Open the file and initialize the SD interface
 sd_id = HDF_SD_START( dir, /read )
 ; Find the index of the sds to read using its name
 Latitude_sds_index = HDF_SD_NAMETOINDEX(sd_id,Latitude_SDS_NAME1)
 if Latitude_sds_index lt 0 then begin
  Latitude_sds_index = HDF_SD_NAMETOINDEX(sd_id,Latitude_SDS_NAME2);
 endif
 
 Longitude_sds_index = HDF_SD_NAMETOINDEX(sd_id,Longitude_SDS_NAME1)
  if Longitude_sds_index lt 0 then begin
   Longitude_sds_index = HDF_SD_NAMETOINDEX(sd_id,Longitude_SDS_NAME2);
  endif
 ; Select it
 Latitude_sds_id = HDF_SD_SELECT( sd_id, Latitude_sds_index )
 Longitude_sds_id = HDF_SD_SELECT( sd_id, Longitude_sds_index )
 
 ; Set the data subset limits. Actually, it is read from the first element, so "start" is [0,0]. X_LENGTH elements will be read along the X axis and Y_LENGTH along Y.
 start=INTARR(2) ; the start position of the data to be read
 start[0] = 0
 start[1] = 0
 ;edges=INTARR(2) ; the number of elements to read in each direction
 ;edges[0] = X_LENGTH
 ;edges[1] = Y_LENGTH
 ; Read the data : you can notice that here, it is not needed to allocate the data array yourself
 HDF_SD_GETDATA, Latitude_sds_id, Latitude_data
 HDF_SD_GETDATA, Longitude_sds_id, Longitude_data
 dims = size(Longitude_data)
 ;Long_Lat = make_array(2,dims[1],/float);
 ;HDF_SD_GETDATA, sds_id, data, start = start, count = edges
 
 ; print them on the screen. You can notice that the IDL method HDF_SD_GETDATA swaps the HDF indexes convention [Z,Y,X] to [X,Y,Z]. This method is more efficient on IDL. If you prefer the usual HDF convention, you should better use the set the NOREVERSE keyword when calling HDF_SD_GETDATA 
 ;FOR i=0,(X_LENGTH-1),1 DO BEGIN ; crosses X axis
 ;  FOR j=0,(Y_LENGTH-1),1 DO BEGIN ; crosses Y axis
 ;    PRINT, FORMAT='(I," ",$)', data[i,j]
 ;  ENDFOR
 ;  PRINT,""
 ;ENDFOR
 
 ; end access to SDS
 HDF_SD_ENDACCESS, Latitude_sds_id
 HDF_SD_ENDACCESS, Longitude_sds_id
 ; close the hdf file
 HDF_SD_END, sd_id
 ;print,dims[1];1354 也就是说，实际上X_LENGTH，X_LENGTH可以通过size读出。
 ;print,dims[2];4856
 ;print,dims[3]
 ;print,dims[4]
 
 ;filter_data = uintarr(dims[1], dims[2],dims[3])
 ;imagedata =    BYTSCL(data)
 ;imagedata = bytscl( congrid(data,800,600) );缩小图片。
 
 ;print,imagedata 
  ;loadct, 15;调色板编号不一样。
 ; window, 0, xsize=800, ysize=600, retain=2
 ; tv, imagedata
   end