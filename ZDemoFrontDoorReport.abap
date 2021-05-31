*&---------------------------------------------------------------------*
*& Report  ZADF_DEMO_AZURE_FRONTDOOR
*&
*&---------------------------------------------------------------------*
**---------------Prerequisite------------------------------------------*
** *
** *
**---------------------------------------------------------------------*
** 1.Create your ...*
*&---------------------------------------------------------------------*
REPORT ZADF_DEMO_AZURE_FRONTDOOR.

TYPES: BEGIN OF lty_data,
         id        TYPE    s_carr_id,
         carrid    TYPE    s_carr_id,
         connid    TYPE    string,
         fldate    TYPE    s_date,
         planetype TYPE    s_planetye,
         SEATSMAX  TYPE    int4,
         SEATSOCC  TYPE    int4,
       END OF lty_data.

DATA: it_data        TYPE STANDARD TABLE OF lty_data,
      lv_http_dest   TYPE rfcdest VALUE 'AzureFrontDoor',
      lo_http_client TYPE REF TO if_http_client,
      lo_rest_client TYPE REF TO cl_rest_http_client,
      lo_request     TYPE REF TO if_rest_entity,
      lo_response    TYPE REF TO if_rest_entity,
      lv_url         TYPE string VALUE '/odata/sflight',
      http_status    TYPE string,
      lv_status      TYPE i,
      reason         TYPE string,
      lv_body        TYPE string,
      lv1_string     TYPE string,
      W_LEN          TYPE I.


*Sample data population for sending it to Azure Cosmos
SELECT  connid carrid connid fldate planetype SEATSMAX SEATSOCC
        FROM sflight
        INTO TABLE it_data
        WHERE connid = 64 AND fldate = '20210813'.

    lv1_string = /ui2/cl_json=>serialize( data = it_data compress = abap_false pretty_name = /ui2/cl_json=>pretty_mode-camel_case ).
    W_LEN = STRLEN( lv1_string ) - 2.
    lv1_string = lv1_string+1(W_LEN).

IF sy-subrc EQ 0.

  TRY.

    cl_http_client=>create_by_destination(
      EXPORTING
        destination              = lv_http_dest    " Logical destination (specified in function call)
      IMPORTING
        client                   = lo_http_client    " HTTP Client Abstraction
      EXCEPTIONS
        argument_not_found       = 1
        destination_not_found    = 2
        destination_no_authority = 3
        plugin_not_active        = 4
        internal_error           = 5
        OTHERS                   = 6
    ).

    CREATE OBJECT lo_rest_client
     EXPORTING
      io_http_client = lo_http_client.
      lo_http_client->request->set_version( if_http_request=>co_protocol_version_1_1 ).

      IF lo_http_client IS BOUND AND lo_rest_client IS BOUND.
        cl_http_utility=>set_request_uri(
          EXPORTING
            request = lo_http_client->request    " HTTP Framework (iHTTP) HTTP Request
            uri     = lv_url                     " URI String (in the Form of /path?query-string)
        ).

        lo_request = lo_rest_client->if_rest_client~create_request_entity( ).
        lo_request->set_content_type( iv_media_type = if_rest_media_type=>gc_appl_json ).
        lo_request->set_string_data( lv1_string ).

*     CALL METHOD lo_rest_client->if_rest_client~set_request_header
*       EXPORTING
*         iv_name  = 'auth-token'
*         iv_value = token number. "Set your header .
* POST
        lo_rest_client->if_rest_resource~post( lo_request ).

        lo_response = lo_rest_client->if_rest_client~get_response_entity( ).
        http_status = lv_status = lo_response->get_header_field( '~status_code' ).
        reason = lo_response->get_header_field( '~status_reason' ).
*        response = lo_response->get_string_data( ).
        IF http_status EQ 201 OR http_status EQ 200.
          MESSAGE 'SAP data sent to Azure Cosmos' TYPE 'I'.
        ELSE.
          MESSAGE 'SAP data not sent to Azure Cosmos' TYPE 'E'.
        ENDIF.
        lo_http_client->close( ).
     ENDIF.

  CATCH cx_root INTO DATA(e_txt).
    WRITE: / e_txt->get_text( ).
  ENDTRY.

ENDIF.