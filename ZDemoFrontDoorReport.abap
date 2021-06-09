*&---------------------------------------------------------------------*
*& Report  ZSFLIGHT_DEMO_AZURE_FRONTDOOR
*&
*&---------------------------------------------------------------------*
**---------------Prerequisite------------------------------------------*
** * Destination for AAD and FrontDoor configured
**---------------------------------------------------------------------*
** 1.Request a Bearer token from Azure AD for subsequent REST calls.
** 2.Creates an entry based on SQL filter on SFlight data set in CosmosDB
**   via POST request
*&---------------------------------------------------------------------*
REPORT ZSFLIGHT_DEMO_AZURE_FRONTDOOR.

TYPES: BEGIN OF lty_data,
         id        TYPE    s_carr_id,
         carrid    TYPE    s_carr_id,
         connid    TYPE    string,
         fldate    TYPE    s_date,
         planetype TYPE    s_planetye,
         SEATSMAX  TYPE    int4,
         SEATSOCC  TYPE    int4,
       END OF lty_data,
       BEGIN OF lty_json_data,
         token_type     TYPE    string,
         expires_in     TYPE    i,
         ext_expires_in TYPE    i,
         access_token   TYPE    string,
       END OF lty_json_data.

DATA: it_data          TYPE STANDARD TABLE OF lty_data,
      it_json_data     TYPE STANDARD TABLE OF lty_json_data,
      it_json_line     TYPE lty_json_data,
      lv_http_dest     TYPE rfcdest VALUE 'AzureFrontDoor',
      lv_AAD_dest      TYPE rfcdest VALUE 'AzureADLogin',
      it_params        TYPE tihttpnvp,
      wa_params        TYPE ihttpnvp,
      lo_http_client   TYPE REF TO if_http_client,
      lo_rest_client   TYPE REF TO cl_rest_http_client,
      lo_request       TYPE REF TO if_rest_entity,
      lo_response      TYPE REF TO if_rest_entity,
      lv_url           TYPE string VALUE '/api/odata/Sflight',
      http_status      TYPE string,
      lv_status        TYPE i,
      reason           TYPE string,
      lv_body          TYPE string,
      lv1_string       TYPE string,
      W_LEN            TYPE I,
      json_response    TYPE string,
      lv_token         TYPE string.


*Sample data population for sending it to Azure Cosmos
SELECT  connid carrid connid fldate planetype SEATSMAX SEATSOCC
        FROM sflight
        INTO TABLE it_data
        WHERE connid = 64 AND fldate = '20210813'.
*create JSON from table structure
    lv1_string = /ui2/cl_json=>serialize( data = it_data compress = abap_false pretty_name = /ui2/cl_json=>pretty_mode-camel_case ).
    W_LEN = STRLEN( lv1_string ) - 2.
*unpack json array to avoid conflict when POSTing
    lv1_string = lv1_string+1(W_LEN).

IF sy-subrc EQ 0.

****************Get OAuth token from AAD****************

  TRY.

    wa_params-name = 'scope'.
    wa_params-value =  '<your scope>/.default'.
    APPEND wa_params TO it_params.
    CLEAR wa_params.
    wa_params-name = 'client_id'.
    wa_params-value =  'api://<your client id>'.
    APPEND wa_params TO it_params.
    CLEAR wa_params.
    wa_params-name = 'client_secret'.
    wa_params-value = '<your client secret>'.
    APPEND wa_params TO it_params.
    CLEAR wa_params.
    wa_params-name = 'grant_type'.
    wa_params-value = 'client_credentials'.
    APPEND wa_params TO it_params.
    CLEAR wa_params.

    lv_body = cl_http_utility=>fields_to_string( it_params ).
    CLEAR it_params.

    cl_http_client=>create_by_destination(
      EXPORTING
        destination              = lv_AAD_dest    
      IMPORTING
        client                   = lo_http_client 
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

        lo_request = lo_rest_client->if_rest_client~create_request_entity( ).
        lo_request->set_content_type( iv_media_type = if_rest_media_type=>gc_appl_www_form_url_encoded ).

        lo_request->set_string_data( lv_body ).

        lo_rest_client->if_rest_resource~post( lo_request ).

        lo_response = lo_rest_client->if_rest_client~get_response_entity( ).
        json_response = lo_response->get_string_data( ).
*Make array for ABAP json parser logic
        CONCATENATE '[' json_response ']' into json_response.

* Get access token from JSON payload
        /ui2/cl_json=>deserialize( EXPORTING json = json_response CHANGING data = it_json_data ).
        READ TABLE it_json_data into it_json_line INDEX 1.
        lv_token = it_json_line-access_token.
        CONCATENATE 'Bearer' lv_token INTO lv_token SEPARATED BY SPACE.
      ENDIF.


****************Send message to CosmosDB via AppService and FrontDoor****************

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
            request = lo_http_client->request    
            uri     = lv_url                     
        ).

        lo_request = lo_rest_client->if_rest_client~create_request_entity( ).
        lo_request->set_content_type( iv_media_type = if_rest_media_type=>gc_appl_json ).
        CALL METHOD lo_rest_client->if_rest_client~set_request_header
          EXPORTING
            iv_name  = 'Authorization'
            iv_value = lv_token.

        lo_request->set_string_data( lv1_string ).
* Send POST request
        lo_http_client->propertytype_logon_popup = if_http_client=>co_disabled.
        lo_rest_client->if_rest_resource~post( lo_request ).

        lo_response = lo_rest_client->if_rest_client~get_response_entity( ).
        http_status = lv_status = lo_response->get_header_field( '~status_code' ).
        reason = lo_response->get_header_field( '~status_reason' ).

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