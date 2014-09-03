pro ReadMOD021KM_hdf_2,dir,savepath,Latitude_data=Latitude_data,Band_21=Band_21,Band_22=Band_22,Band_31=Band_31,Band_32=Band_32
 ; Set some constants
 Latitude_SDS_NAME = "Latitude";纬度
 Band_1000 = "EV_1KM_Emissive";波动20-36
 
 ; Open the file and initialize the SD interface
 sd_id = HDF_SD_START( dir, /read )
 ; Find the index of the sds to read using its name
 Latitude_sds_index = HDF_SD_NAMETOINDEX(sd_id,Latitude_SDS_NAME)
 Band_1000_index = HDF_SD_NAMETOINDEX(sd_id,Band_1000)
 ; Select it
 Latitude_sds_id = HDF_SD_SELECT( sd_id, Latitude_sds_index )
 Band_1000_id = HDF_SD_SELECT( sd_id, Band_1000_index )
 
 ; Read the data : you can notice that here, it is not needed to allocate the data array yourself
 HDF_SD_GETDATA, Latitude_sds_id, Latitude_data
 HDF_SD_GETDATA, Band_1000_id, Band_1000_data

 ; end access to SDS
 HDF_SD_ENDACCESS, Latitude_sds_id
 HDF_SD_ENDACCESS, Band_1000_id
 ; close the hdf file
 HDF_SD_END, sd_id

 dims = size(Band_1000_data)
 height = dims[1]
 width = dims[2]
 Band_21 = INTARR(height,width)
 Band_22 = INTARR(height,width)
 Band_31 = INTARR(height,width)
 Band_32 = INTARR(height,width)
 rgb = INTARR(3,height,width)
 FOR i = 0,height-1,1 DO BEGIN
    FOR j = 0,width-1,1 DO BEGIN
      Band_21[i,j] = Band_1000_data[i,j,1]
      Band_22[i,j] = Band_1000_data[i,j,2]
      Band_31[i,j] = Band_1000_data[i,j,10]
      Band_32[i,j] = Band_1000_data[i,j,11]
   ENDFOR
 ENDFOR
 rgb[0,*,*] = Band_22
 rgb[1,*,*] = Band_31
 rgb[2,*,*] = Band_32
 tif_file = savepath +"\原始数据层.tif"
 ;WRITE_TIFF, tif_file, rgb, /SHORT, /APPEND
 end