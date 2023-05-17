cd ffmpeg
ffmpeg -i rtmp://localhost/living/test ^
-max_muxing_queue_size 9999  ^
-async 1 -vf yadif -g 29.97 -r 23 ^
-b:v:0 5250k -c:v libx264  -filter:v:0 "scale=426:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1 ^
-b:v:1 4200k -c:v libx264 -filter:v:1 "scale=640:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  ^
-b:v:1 3150k -c:v libx264 -filter:v:2 "scale=1280:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1 ^
-b:a:0 256k -b:a:0 192k -b:a:0 128k -c:a aac -ar 48000  -map 0:v -map 0:a:0 -map 0:v -map 0:a:0 -map 0:v -map 0:a:0 ^
-f hls ^
-var_stream_map "v:0,a:0  v:1,a:1 v:2,a:2" ^
-master_pl_name  index.m3u8 ^
-t 30000 -hls_time 10 ^
 -hls_init_time 4 -hls_list_size 0 ^
-master_pl_publish_rate 10 ^
-hls_flags delete_segments+discont_start+split_by_time "../live/test/vs%%v/manifest.m3u8" 
pause