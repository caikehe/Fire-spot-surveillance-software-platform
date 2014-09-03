pro ReadMOD021KM_hdf_1,dir,Longitude_data=Longitude_data,Band_1=Band_1,Band_2=Band_2,Band_5=Band_5,Band_7=Band_7

 ; Set some constants
 Longitude_SDS_NAME = "Longitude";经度
 Band_250 = "EV_250_Aggr1km_RefSB";波段1-2
 Band_500 = "EV_500_Aggr1km_RefSB";波段3-7
 
 ; Open the file and initialize the SD interface
 sd_id = HDF_SD_START( dir, /read )
 ; Find the index of the sds to read using its name
 Longitude_sds_index = HDF_SD_NAMETOINDEX(sd_id,Longitude_SDS_NAME)
 Band_250_index = HDF_SD_NAMETOINDEX(sd_id,Band_250)
 Band_500_index = HDF_SD_NAMETOINDEX(sd_id,Band_500)
 ; Select it
 Longitude_sds_id = HDF_SD_SELECT( sd_id, Longitude_sds_index )
 Band_250_id = HDF_SD_SELECT( sd_id, Band_250_index )
 Band_500_id = HDF_SD_SELECT( sd_id, Band_500_index )
 
 ; Read the data : you can notice that here, it is not needed to allocate the data array yourself
 HDF_SD_GETDATA, Longitude_sds_id, Longitude_data
 HDF_SD_GETDATA, Band_250_id, Band_250_data
 HDF_SD_GETDATA, Band_500_id, Band_500_data

 ; end access to SDS
 HDF_SD_ENDACCESS, Longitude_sds_id
 HDF_SD_ENDACCESS, Band_250_id
 HDF_SD_ENDACCESS, Band_500_id
 ; close the hdf file
 HDF_SD_END, sd_id

 dims = size(Band_250_data)
 height = dims[1]
 width = dims[2]
 Band_1 = INTARR(height,width)
 Band_2 = INTARR(height,width)
 Band_5 = INTARR(height,width)
 Band_7 = INTARR(height,width)
 rgb = INTARR(3,height,width)
 FOR i = 0,height-1,1 DO BEGIN
    FOR j = 0,width-1,1 DO BEGIN
      Band_1[i,j] = Band_250_data[i,j,0]
      Band_2[i,j] = Band_250_data[i,j,1]
      Band_5[i,j] = Band_500_data[i,j,2]
      Band_7[i,j] = Band_500_data[i,j,4]
   ENDFOR
 ENDFOR
 end